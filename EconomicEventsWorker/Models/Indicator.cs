namespace EconomicEventsWorker.Models
{
    public class Indicator
    {
        public Guid Id { get; set; }
        public string Name { get; set; }      // CPI, GDP, etc.
        public string SeriesId { get; set; }  // FRED series_id
        public string LastValue { get; set; } // Последно изпратена стойност
        public DateTime? LastDate { get; set; }
    }
}
