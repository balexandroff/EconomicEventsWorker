namespace EconomicEventsWorker.Models
{
    public class Observation
    {
        public int Id { get; set; }
        public int IndicatorId { get; set; }
        public string Value { get; set; }
        public DateTime Date { get; set; }
        public Indicator Indicator { get; set; }
    }
}
