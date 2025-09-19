using EconomicEventsWorker.Database;
using EconomicEventsWorker.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;

namespace EconomicEventsWorker.Services
{
    public class InvestingComScraper
    {
        private readonly IOptions<AppSettings> _options;
        private readonly IServiceProvider _services;
        private readonly DiscordNotifier _discordNotifier;
        private readonly ILogger<InvestingComScraper> _logger;
        private readonly IDictionary<string, string> _indicators;

        public InvestingComScraper(IServiceProvider services, DiscordNotifier discordNotifier, IOptions<AppSettings> options, ILogger<InvestingComScraper> logger)
        {
            _services = services;
            _discordNotifier = discordNotifier;
            _options = options;
            _logger = logger;
            _indicators = new Dictionary<string, string>()
            {
                { "FEDFUNDS", _options.Value.InvestingCom.RateUrl },
                { "CPI", _options.Value.InvestingCom.CPIUrl },
                { "PPIACO", _options.Value.InvestingCom.PPIUrl },
                { "UNRATE", _options.Value.InvestingCom.UnemploymentRateUrl },
                { "ICSA", _options.Value.InvestingCom.InitialJoblessClaimsUrl }
            };
        }

        public async Task ScrapeLatestFEDInterestRatesAndNotify()
        {
            using (var scope = _services.CreateScope())
            {
                try
                {
                    using var db = scope.ServiceProvider.GetRequiredService<EconomicContext>();

                    db.Database.EnsureCreated();

                    foreach (var indicator in _indicators)
                    {
                        var dbIndicator = db.Indicators.FirstOrDefault(ind => ind.SeriesId == indicator.Key);
                        if (dbIndicator != null)
                        { 
                            using var handler = new HttpClientHandler
                            {
                                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                            };
                            using var client = new HttpClient(handler);
                            client.DefaultRequestHeaders.Add("User-Agent",
                                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117 Safari/537.36");

                            var html = await client.GetStringAsync(indicator.Value);

                            var doc = new HtmlDocument();
                            doc.LoadHtml(html);


                            // Get the first row (latest decision)
                            var tableContainerId = indicator.Value.Split("-").LastOrDefault();
                            var rows = doc.DocumentNode.SelectNodes($"//table[@id='eventHistoryTable{tableContainerId}']//tr");
                            if (rows != null)
                            {
                                foreach (var row in rows)
                                {
                                    var cells = row.SelectNodes("./td");
                                    if (cells != null && cells.Count >= 5)
                                    {
                                        var date = cells[0].InnerText.Trim();
                                        var time = cells[1].InnerText.Trim();
                                        var actual = cells[2].InnerText.Trim();
                                        var forecast = cells[3].InnerText.Trim();
                                        var previous = cells[4].InnerText.Trim();

                                        var className = cells[2].FirstChild?.GetAttributes("class")?.FirstOrDefault()?.Value;
                                        var title = cells[2].FirstChild?.GetAttributes("title")?.FirstOrDefault()?.Value;

                                        int dateIndex = date.IndexOf('(');
                                        if (dateIndex >= 0)
                                        {
                                            date = date.Substring(0, dateIndex).Trim();
                                        }

                                        var observation = new Observation
                                        {
                                            Id = Guid.NewGuid(),
                                            Date = DateTime.Parse($"{date} {time}"),
                                            Value = actual,
                                            Indicator = dbIndicator,
                                            IndicatorId = dbIndicator.Id
                                        };

                                        // Проверка дали вече е изпратено
                                        if (dbIndicator.LastValue != observation.Value &&
                                            !string.IsNullOrEmpty(actual.Replace("&nbsp;", string.Empty)) &&
                                            (dbIndicator.LastDate is null || dbIndicator.LastDate <= observation.Date))
                                        {
                                            await _discordNotifier.SendEventUpdatesAsync(observation, previous, forecast, GetFont(className), title);

                                            // Запазваме в база
                                            dbIndicator.LastValue = observation.Value;
                                            dbIndicator.LastDate = observation.Date;
                                            db.Observations.Add(new Observation
                                            {
                                                IndicatorId = dbIndicator.Id,
                                                Date = observation.Date,
                                                Value = observation.Value
                                            });

                                            await db.SaveChangesAsync();
                                        }

                                        //Console.WriteLine($"Latest Fed Decision → {date} {time} | Actual: {actual} | Forecast: {forecast} | Previous: {previous}");
                                    }
                                }
                            }

                            await Task.Delay(3000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error while loadiong Investing Interest Rates data: {ex}");
                }
            }
        }

        private int? GetFont(string fontName)
        {
            switch (fontName)
            {
                case "blackFont":
                    return 0x000000;
                case "redFont":
                    return 0xFF0000;
                case "greenFont":
                    return 0x00FF00;
                default:
                    return 0x000000;
            }
        }
    }
}
