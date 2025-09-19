using EconomicEventsWorker.Database;
using EconomicEventsWorker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EconomicEventsWorker.Services
{
    public class FREDEventsScraper
    {
        private readonly IOptions<AppSettings> _options;
        private readonly IServiceProvider _services;
        private readonly DiscordNotifier _discordNotifier;
        private readonly ILogger<FREDEventsScraper> _logger;
        private readonly string _apiKey;

        public FREDEventsScraper(IServiceProvider services, DiscordNotifier discordNotifier, IOptions<AppSettings> options, ILogger<FREDEventsScraper> logger)
        {
            _services = services;
            _discordNotifier = discordNotifier;
            _options = options;
            _logger = logger;
            _apiKey = Environment.GetEnvironmentVariable("FRED_API_KEY") ?? throw new ArgumentNullException("Discord API Key should be provided.");
        }

        public async Task ScrapeLatestEventsAndNotify()
        {
            using (var scope = _services.CreateScope())
            {
                try
                {
                    using var db = scope.ServiceProvider.GetRequiredService<EconomicContext>();

                    db.Database.EnsureCreated();

                    foreach (var indicator in await db.Indicators.ToListAsync())
                    {
                        var observations = await GetLatestObservation(indicator.SeriesId);
                        var latest = observations.LastOrDefault();
                        if (latest == null) continue;

                        // Проверка дали вече е изпратено
                        if (indicator.LastValue != latest.Value)
                        {
                            latest.Indicator.Id = Guid.NewGuid();
                            latest.Indicator.Name = indicator.Name;
                            latest.Indicator.LastValue = latest.Value;

                            await _discordNotifier.SendEventUpdatesAsync(latest);

                            // Запазваме в база
                            indicator.LastValue = latest.Value;
                            indicator.LastDate = latest.Date;
                            db.Observations.Add(new Observation
                            {
                                IndicatorId = indicator.Id,
                                Date = latest.Date,
                                Value = latest.Value
                            });

                            await db.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex) {
                    _logger.LogError($"Error while loadiong FRED data: {ex}");
                }
            }
        }

        private async Task<List<Observation>> GetLatestObservation(string seriesId)
        {
            Thread.Sleep(3000);

            List<Observation> response = new List<Observation>();

            try
            {
                _logger.LogInformation($"Start fetching data for SerieId: \"{seriesId}\"");

                var url = _options.Value.FRED.ApiUrl.Replace("{API_KEY}", _apiKey)
                                                    .Replace("{seriesId}", seriesId)
                                                    .Replace("{observation_start}", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"))
                                                    .Replace("{observation_end}", DateTime.Now.ToString("yyyy-MM-dd"))
                                                    .Replace("{realtime_start}", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"))
                                                    .Replace("{realtime_end}", DateTime.Now.ToString("yyyy-MM-dd"));
                var json = await (new HttpClient()).GetStringAsync(url);
                //var response = await (new HttpClient()).GetFromJsonAsync<FredApiResponse>(url);
                //return response?.Observations?.Select(o => new FredObservation
                //{
                //    Date = DateTime.Parse(o.date),
                //    Value = o.value
                //}).ToArray() ?? Array.Empty<FredObservation>();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var fredData = JsonSerializer.Deserialize<FredObservationResponse>(json, options);

                response = fredData.Observations.Select(e => new Observation
                {
                    Id = Guid.NewGuid(),
                    Indicator = new Indicator
                    {
                        SeriesId = seriesId,
                        LastValue = e.Value,
                        LastDate = DateTime.Parse(e.Date)
                    },
                    Value = e.Value,
                    Date = DateTime.Parse(e.Date)
                }).ToList();

                _logger.LogInformation($"End fetching data for SerieId: \"{seriesId}\"");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while loadiong FRED observation data: {ex}");
            }

            return await Task.FromResult(response);

        }
    }
}
