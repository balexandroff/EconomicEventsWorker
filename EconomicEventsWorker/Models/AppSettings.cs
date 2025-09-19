namespace EconomicEventsWorker.Models
{
    public class AppSettings
    {
        public DiscordSettings Discord { get; set; } = new();
        public FREDSettings FRED { get; set; } = new();
        public TradingEconomicsSettings TradingEconomics { get; set; } = new();
        public InvestingComSettings InvestingCom { get; set; } = new();
        public int CheckIntervalMinutes { get; set; } = 10;
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

    public class InvestingComSettings
    {
        public string RateUrl { get; set; } = "";
        public string CPIUrl { get; set; } = "";
        public string PPIUrl { get; set; } = "";
        public string UnemploymentRateUrl { get; set; } = "";
        public string InitialJoblessClaimsUrl { get; set; } = "";
    }
}
