namespace EconomicEventsWorker.Models
{
    public class EconomicEvent
    {
        public Guid Id { get; set; }  // <-- Primary key
        public string Country { get; set; } = "";
        public string Category { get; set; } = "";
        public string Event { get; set; } = "";
        public string Reference { get; set; } = "";
        public string Source { get; set; } = "";
        public string Actual { get; set; } = "";
        public string Previous { get; set; } = "";
        public string Forecast { get; set; } = "";
        public DateTime Date { get; set; }
    }
}
