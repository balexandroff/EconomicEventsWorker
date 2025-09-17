namespace EconomicEventsWorker.Models
{
    public class AppSettings
    {
        public TelegramSettings Telegram { get; set; } = new();
        public DiscordSettings Discord { get; set; } = new();
        public FREDSettings FRED { get; set; } = new();
        public TradingEconomicsSettings TradingEconomics { get; set; } = new();
        public int CheckIntervalMinutes { get; set; } = 10;
    }

    public class TelegramSettings
    {
        public string BotToken { get; set; } = "";
        public string ChatId { get; set; } = "";
    }

    public class DiscordSettings
    {
        public string WebhookUrl { get; set; } = "";
    }

    public class FREDSettings
    {
        public string ApiUrl { get; set; } = "";
    }

    public class TradingEconomicsSettings
    {
        public string ApiUrl { get; set; } = "";
    }
}
