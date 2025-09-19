namespace EconomicEventsWorker.Models
{
    public class Observation
    {
        public Guid Id { get; set; }
        public Guid IndicatorId { get; set; }
        public string Value { get; set; }
        public DateTime Date { get; set; }
        public Indicator Indicator { get; set; }
    }
}
