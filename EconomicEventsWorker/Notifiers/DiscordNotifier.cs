using EconomicEventsWorker.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

public class DiscordNotifier
{
    private readonly IOptions<AppSettings> _options;
    private readonly ILogger<DiscordNotifier> _logger;
    private readonly string _apiKey;

    public DiscordNotifier(IOptions<AppSettings> options, ILogger<DiscordNotifier> logger)
    {
        _options = options;
        _logger = logger;
        _apiKey = Environment.GetEnvironmentVariable("DISCORD_API_KEY") ?? throw new ArgumentNullException("Discord API Key should be provided.");
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

    public async Task SendUpcomingEventsAsync(List<WeeklyEvent> upcomingEvents)
    {
        try
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
            var response = await client.PostAsync(_options.Value.Discord.WebhookUrl.Replace("{API_KEY}", _apiKey), content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while sending Discord upcoming events: {ex}");
        }
    }

    public async Task SendEventUpdatesAsync(Observation @event, string previousValue = null, string forecastValue = null, int? color = null, string helperText = null)
    {
        try
        {
            color = color ?? 0x00FF00; // green for example

            var payload = new
            {
                embeds = new[]
                {
                new
                {
                    title = $"📊 **{@event.Indicator.Name} Update**",
                    description = $"📅 {@event.Date:yyyy-MM-dd HH:mm}\n" +
                                  $"- Value: {@event.Value}" + $"{(!string.IsNullOrEmpty(helperText) ? " - *" + helperText + "*" : string.Empty)}" +
                                  $"{(!string.IsNullOrEmpty(previousValue) ? $"\n- Previous Value: {previousValue}" : string.Empty)}" +
                                  $"{(!string.IsNullOrEmpty(forecastValue) ? $"\n- Forecast Value: {forecastValue}" : string.Empty)}",
                    color = color,
                    timestamp = @event.Date.ToUniversalTime().ToString("o")
                }
            }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync(_options.Value.Discord.WebhookUrl.Replace("{API_KEY}", _apiKey), content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while sending Discord event updates: {ex}");
        }
    }
}