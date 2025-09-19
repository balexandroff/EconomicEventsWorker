using EconomicEventsWorker.Database;
using EconomicEventsWorker.Models;
using EconomicEventsWorker.Services;
using Microsoft.EntityFrameworkCore;

namespace EconomicEventsWorker.Notifiers
{
    public class WeeklyNotifier
    {
        private readonly CalendarEventsScraper _scraper;
        private readonly IServiceProvider _services;
        private readonly DiscordNotifier _discordNotifier;
        private readonly ILogger<WeeklyNotifier> _logger;

        public WeeklyNotifier(CalendarEventsScraper scraper, DiscordNotifier discordNotifier,  IServiceProvider services, ILogger<WeeklyNotifier> logger)
        {
            _scraper = scraper;
            _services = services;
            _discordNotifier = discordNotifier;
            _logger = logger;
        }

        public async Task SendWeeklyNotificationIfNeeded()
        {
            try {
                using (var scope = _services.CreateScope())
                {
                    await _scraper.TryFeedWeeklyEvents();

                    using var db = scope.ServiceProvider.GetRequiredService<EconomicContext>();

                    db.Database.EnsureCreated();

                    // Започваме седмицата в неделя (може да се промени ако искаш понеделник)
                    var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

                    bool alreadySent = await db.NotificationLogs
                        .AnyAsync(n => n.Type == "Weekly" && n.SentDate >= weekStart);

                    if (alreadySent) return; // Вече е пращано -> не пращаме

                    var upcoming = await db.WeeklyEvents
                        .Where(w => w.ScheduledDate >= DateTime.Today && w.ScheduledDate < DateTime.Today.AddDays(7))
                        .OrderBy(w => w.ScheduledDate)
                        .ToListAsync();

                    if (!upcoming.Any()) return;

                    await _discordNotifier.SendUpcomingEventsAsync(upcoming);

                    // Логваме, че сме пратили седмичната нотификация
                    db.NotificationLogs.Add(new NotificationLog
                    {
                        Type = "Weekly",
                        SentDate = DateTime.Now
                    });
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while sending weekly notification: {ex}");
            }
        }
    }
}
