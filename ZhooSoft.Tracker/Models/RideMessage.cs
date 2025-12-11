namespace ZhooSoft.Tracker.Models
{
    public class RideMessage
    {
        #region Properties

        public string Message { get; set; }

        public int RideRequestId { get; set; }

        public int SenderId { get; set; }

        public string SenderType { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        #endregion
    }
}
