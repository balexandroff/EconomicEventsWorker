namespace EconomicEventsWorker.Models
{
    public class WeeklyEvent
    {
        public Guid Id { get; set; }
        public string Source { get; set; }
        public string Name { get; set; }    // Име на показателя
        public DateTime ScheduledDate { get; set; }
    }
}
