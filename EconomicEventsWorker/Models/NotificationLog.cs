namespace EconomicEventsWorker.Models
{
    public class NotificationLog
    {
        public Guid Id { get; set; }
        public string Type { get; set; } // "Weekly"
        public DateTime SentDate { get; set; }
    }
}
