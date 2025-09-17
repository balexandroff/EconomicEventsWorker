namespace EconomicEventsWorker.Models
{
    public class NotificationLog
    {
        public int Id { get; set; }
        public string Type { get; set; } // "Weekly"
        public DateTime SentDate { get; set; }
    }
}
