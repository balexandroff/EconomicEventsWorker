using EconomicEventsWorker.Database;
using EconomicEventsWorker.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EconomicEventsWorker.Services
{
    public class TradingEconomicsScraper
    {
        private readonly IOptions<AppSettings> _options;
        private readonly IServiceProvider _services;
        private readonly DiscordNotifier _discordNotifier;
        private readonly string _tradingEconomicsApiKey;

        public TradingEconomicsScraper(IOptions<AppSettings> options, DiscordNotifier discordNotifier, IServiceProvider services)
        {
            DotNetEnv.Env.Load();
            _options = options;
            _discordNotifier = discordNotifier;
            _services = services;
            _tradingEconomicsApiKey = Environment.GetEnvironmentVariable("TRADING_ECONOMICS_API_KEY") ?? throw new ArgumentNullException("TRADING_ECONOMICS_API_KEY environment variable is not set");
        }

        public async Task ScrapeEvents()
        {
            var url = _options.Value.TradingEconomics.ApiUrl.Replace("{API_KEY}", _tradingEconomicsApiKey);
            var json = await (new HttpClient()).GetStringAsync(url);

            var events = JsonSerializer.Deserialize<List<EconomicEvent>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (events == null) return;

            var today = DateTime.UtcNow.Date;

            var todaysEvents = events
                //.Where(e => e.Country == "United States" && e.Date.Date == today)
                .ToList();

            await SaveToDatabaseAsync(todaysEvents);

            foreach (var ev in todaysEvents)
            {
                var msg = $"📊 {ev.Event}\n" +
                          $"📅 {ev.Date}\n" +
                          $"Forecast: {ev.Forecast}, Actual: {ev.Actual}, Previous: {ev.Previous}";

                //await SendTelegramMessage(msg);
                //await SendDiscordMessage(msg);
                //await _discordNotifier.SendMessageAsync(ev);
            }

            //await SendDiscordMessage("Check for economic events finished!");

        }

        private async Task SaveToDatabaseAsync(List<EconomicEvent> events)
        {
            using (var scope = _services.CreateScope())
            {
                using var db = scope.ServiceProvider.GetRequiredService<EconomicContext>();

                db.Database.EnsureCreated();

                for (int i = events.Count - 1; i >= 0; i--)
                {
                    var @event = events[i];
                    if (!db.EconomicEvents.Where(e => e.Country == @event.Country &&
                                           e.Date == @event.Date &&
                                           e.Category == @event.Category &&
                                           e.Reference == @event.Reference &&
                                           e.Event == @event.Event &&
                                           e.Forecast == @event.Forecast &&
                                           e.Previous == @event.Previous &&
                                           e.Actual == @event.Actual &&
                                           e.Source == @event.Source).Any())
                    {
                        @event.Id = Guid.NewGuid();
                        db.EconomicEvents.Add(@event);
                    }
                    else
                    {
                        events.Remove(@event);
                    }
                }

                await db.SaveChangesAsync();
            }
        }
    }
}
