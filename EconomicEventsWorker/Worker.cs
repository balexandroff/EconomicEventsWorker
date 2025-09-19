using EconomicEventsWorker.Models;
using EconomicEventsWorker.Notifiers;
using EconomicEventsWorker.Services;
using Microsoft.Extensions.Options;

namespace EconomicEventsWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOptions<AppSettings> _options;
        private readonly WeeklyNotifier _weeklyNotifier;
        private readonly FREDEventsScraper _fredScraper;
        private readonly InvestingComScraper _investingInterestRateScraper;

        public Worker(IServiceProvider services, WeeklyNotifier weeklyNotifier, FREDEventsScraper fredScraper, InvestingComScraper investingInterestRateScraper, ILogger<Worker> logger, IOptions<AppSettings> options)
        {
            _logger = logger;
            _options = options;
            _weeklyNotifier = weeklyNotifier;
            _fredScraper = fredScraper;
            _investingInterestRateScraper = investingInterestRateScraper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromMinutes(_options.Value.CheckIntervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckEconomicEvents();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking events");
                }

                await Task.Delay(interval, stoppingToken);
            }
        }

        private async Task CheckEconomicEvents()
        {
            // 1️⃣ Седмична нотификация - проверка дали вече е пращана тази седмица
            await _weeklyNotifier.SendWeeklyNotificationIfNeeded();

            //Try to notify for new events
            await _fredScraper.ScrapeLatestEventsAndNotify();
            await _investingInterestRateScraper.ScrapeLatestFEDInterestRatesAndNotify();
        }
    }
}
