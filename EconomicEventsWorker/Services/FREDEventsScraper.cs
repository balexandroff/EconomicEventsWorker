using EconomicEventsWorker.Database;
using EconomicEventsWorker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EconomicEventsWorker.Services
{
    public class FREDEventsScraper
    {
        private readonly IOptions<AppSettings> _options;
        private readonly IServiceProvider _services;
        private readonly DiscordNotifier _discordNotifier;

        public FREDEventsScraper(IServiceProvider services, DiscordNotifier discordNotifier, IOptions<AppSettings> options)
        {
            _services = services;
            _discordNotifier = discordNotifier;
            _options = options;
        }

        public async Task ScrapeLatestEventsAndNotify()
        {
            using (var scope = _services.CreateScope())
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
                        latest.Indicator = indicator;
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
        }

        private async Task<List<Observation>> GetLatestObservation(string seriesId)
        {
            var url = string.Format(_options.Value.FRED.ApiUrl, seriesId);
            var json = await (new HttpClient()).GetStringAsync(url);
            //var response = await (new HttpClient()).GetFromJsonAsync<FredApiResponse>(url);
            //return response?.Observations?.Select(o => new FredObservation
            //{
            //    Date = DateTime.Parse(o.date),
            //    Value = o.value
            //}).ToArray() ?? Array.Empty<FredObservation>();

            var events = JsonSerializer.Deserialize<List<Observation>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return await Task.FromResult(events);
        }
    }
}
