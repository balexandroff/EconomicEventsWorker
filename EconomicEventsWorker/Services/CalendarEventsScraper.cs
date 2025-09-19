using EconomicEventsWorker.Database;
using EconomicEventsWorker.Models;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EconomicEventsWorker.Services
{
    public class CalendarEventsScraper
    {
        private readonly IServiceProvider _services;
        private readonly HttpClient _httpClient;
        private readonly ILogger<CalendarEventsScraper> _logger;

        public CalendarEventsScraper(IServiceProvider services, ILogger<CalendarEventsScraper> logger)
        {
            _services = services;
            _httpClient = new HttpClient();
            _logger = logger;
        }

        public async Task TryFeedWeeklyEvents()
        {
            var blsEvents = await ScrapeBlsAsync();
            var beaEvents = await ScrapeBeaAsync();

            await SaveEventsToDBAsync(blsEvents);
            await SaveEventsToDBAsync(beaEvents);
        }

        private async Task<List<WeeklyEvent>> ScrapeBlsAsync()
        {
            var events = new List<WeeklyEvent>();

            try
            {
                var url = $"https://www.bls.gov/schedule/{DateTime.Now.ToString("yyyy")}/{DateTime.Now.ToString("MM")}_sched.htm";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent",
                                                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                                                "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

                var html = await client.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);


                // намираме всички клетки от календара
                var cells = doc.DocumentNode.SelectNodes("//table[contains(@class,'release-calendar')]//td");
                if (cells == null) return events;

                foreach (var cell in cells)
                {
                    var dayNode = cell.SelectSingleNode(".//p[@class='day']");
                    if (dayNode == null) continue;

                    var dayText = dayNode.InnerText.Trim();
                    if (!int.TryParse(dayText, out var day)) continue;

                    // всички параграфи след деня
                    var paragraphs = cell.SelectNodes(".//p[not(@class='day')]");
                    if (paragraphs == null) continue;

                    foreach (var p in paragraphs)
                    {
                        var paragraphHtml = p.InnerHtml.Trim();
                        if (string.IsNullOrWhiteSpace(paragraphHtml)) continue;

                        // разделяме по нов реди
                        var parts = paragraphHtml.Split("</strong>", StringSplitOptions.RemoveEmptyEntries).Select(p => Regex.Replace(p.Replace("<br>", " "), "<.*?>", string.Empty)).ToArray();
                        if (parts.Length < 2) continue;

                        var title = parts[0].Trim();
                        var period = parts.Length > 1 ? parts[1].Trim() : "";
                        var time = parts.Length > 2 ? parts[2].Trim() : "";

                        // предполагаме, че календарът е за текущия месец/година
                        var monthYearNode = doc.DocumentNode.SelectSingleNode("//h2[contains(text(),'2025')]");
                        var monthYear = monthYearNode?.InnerText.Trim() ?? "September 2025";
                        DateTime releaseDate;
                        if (!DateTime.TryParse($"{day} {monthYear} {time}", out releaseDate))
                        {
                            // ако няма час, дава default 08:30
                            DateTime.TryParse($"{day} {monthYear} 08:30 AM", out releaseDate);
                        }

                        events.Add(new WeeklyEvent
                        {
                            Id = Guid.NewGuid(),
                            Source = "BLS",
                            Name = $"{title} ({period})",
                            ScheduledDate = releaseDate
                        });
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error while loading BLS calendar events: {ex}");
            }

            return events;
        }

        private async Task<List<WeeklyEvent>> ScrapeBeaAsync()
        {
            var events = new List<WeeklyEvent>();

            try
            {
                var url = "https://www.bea.gov/news/schedule";
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent",
                                                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                                                "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

                var html = await client.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);


                // взимаме всички редове с клас scheduled-releases-type-press
                var rows = doc.DocumentNode.SelectNodes("//tr[contains(@class,'scheduled-releases-type-press')]");
                if (rows == null) return events;

                foreach (var row in rows)
                {
                    var dateNode = row.SelectSingleNode(".//td[contains(@class,'scheduled-date')]");
                    var titleNode = row.SelectSingleNode(".//td[contains(@class,'release-title')]");

                    if (dateNode == null || titleNode == null) continue;

                    var dateText = dateNode.InnerText.Trim();
                    var title = titleNode.InnerText.Trim();

                    // Пример: "September 26" + "8:30 AM"
                    // Взимаме всичко с newlines
                    var parts = dateText.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                    string datePart = parts.Length > 0 ? parts[0].Trim() : "";
                    string timePart = parts.Length > 1 ? parts[1].Trim() : "8:30 AM";

                    DateTime releaseDate;
                    if (!DateTime.TryParse($"{datePart} {DateTime.Now.Year} {timePart}", out releaseDate))
                    {
                        continue;
                    }

                    events.Add(new WeeklyEvent
                    {
                        Id = Guid.NewGuid(),
                        Source = "BEA",
                        Name = title,
                        ScheduledDate = releaseDate
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while loading BEA calendar events: {ex}");
            }

            return events;
        }

        private async Task SaveEventsToDBAsync(List<WeeklyEvent> events)
        {
            using (var scope = _services.CreateScope())
            {
                using var db = scope.ServiceProvider.GetRequiredService<EconomicContext>();

                db.Database.EnsureCreated();

                foreach (var item in events)
                {
                    if (!db.WeeklyEvents.Where(we => we.Source == item.Source &&
                                                    we.Name == item.Name &&
                                                    we.ScheduledDate == item.ScheduledDate)
                                       .Any())
                        db.WeeklyEvents.Add(item);
                }

                await db.SaveChangesAsync();
            }
        }
    }
}
