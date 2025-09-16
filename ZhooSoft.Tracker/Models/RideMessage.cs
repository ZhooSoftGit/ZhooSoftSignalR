namespace ZhooSoft.Tracker.Models
{
    public class RideMessage
    {
        public int BookingRequestId { get; set; }
        public int SenderId { get; set; }
        public string SenderType { get; set; } // "user" or "driver"
        public string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
