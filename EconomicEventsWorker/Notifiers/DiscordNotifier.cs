using EconomicEventsWorker.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class DiscordNotifier
{
    private readonly IOptions<AppSettings> _options;

    public DiscordNotifier(IOptions<AppSettings> options)
    {
        _options = options;
    }

    //public async Task SendMessageAsync(EconomicEvent @event)
    //{
    //    var color = 0x00FF00; // green for example

    //    var payload = new
    //    {
    //        embeds = new[]
    //        {
    //            new
    //            {
    //                title = @event.Event,
    //                description = $"📅 {@event.Date:yyyy-MM-dd HH:mm}\n" +
    //                              $"Forecast: {@event.Forecast}\n" +
    //                              $"Actual: {@event.Actual}\n" +
    //                              $"Previous: {@event.Previous}",
    //                color = color,
    //                timestamp = @event.Date.ToUniversalTime().ToString("o")
    //            }
    //        }
    //    };

    //    var json = JsonSerializer.Serialize(payload);
    //    var content = new StringContent(json, Encoding.UTF8, "application/json");

    //    using var client = new HttpClient();
    //    var response = await client.PostAsync(_options.Value.Discord.WebhookUrl, content);
    //    response.EnsureSuccessStatusCode();
    //}

    public async Task SendTestNotificationAsync(List<WeeklyEvent> upcomingEvents)
    {
        var color = 0x00FF00; // green for example

        string message = string.Empty;// "📅 **Economic Calendar for this week:**\n";
        foreach (var @event in upcomingEvents)
            message += $"- {@event.Name} → {@event.ScheduledDate:ddd dd MMM yyyy}\n";

        var payload = new
        {
            embeds = new[]
            {
                new
                {
                    title = "🚨🚨🚨TEST MESSAGE FROM HOSTED APP🚨🚨🚨 **Economic Calendar for this week:**",
                    description = message,
                    color = color,
                    timestamp = DateTime.Now.ToUniversalTime().ToString("o")
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var client = new HttpClient();
        var response = await client.PostAsync(_options.Value.Discord.WebhookUrl, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendUpcomingEventsAsync(List<WeeklyEvent> upcomingEvents)
    {
        var color = 0x00FF00; // green for example

        string message = string.Empty;// "📅 **Economic Calendar for this week:**\n";
        foreach (var @event in upcomingEvents)
            message += $"- {@event.Name} → {@event.ScheduledDate:ddd dd MMM yyyy}\n";

        var payload = new
        {
            embeds = new[]
            {
                new
                {
                    title = "📅 **Economic Calendar for this week:**",
                    description = message,
                    color = color,
                    timestamp = DateTime.Now.ToUniversalTime().ToString("o")
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var client = new HttpClient();
        var response = await client.PostAsync(_options.Value.Discord.WebhookUrl, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendEventUpdatesAsync(Observation @event)
    {
        var color = 0x00FF00; // green for example

        var payload = new
        {
            embeds = new[]
            {
                new
                {
                    title = $"📊 **{@event.Indicator.Name} Update**",
                    description = $"📅 {@event.Date:yyyy-MM-dd HH:mm}\n" +
                                  $"🟢 Value: {@event.Value}",
                    color = color,
                    timestamp = @event.Date.ToUniversalTime().ToString("o")
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var client = new HttpClient();
        var response = await client.PostAsync(_options.Value.Discord.WebhookUrl, content);
        response.EnsureSuccessStatusCode();
    }
}