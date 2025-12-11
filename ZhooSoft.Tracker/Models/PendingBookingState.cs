namespace ZhooSoft.Tracker.Models
{
    public class PendingBookingState
    {
        #region Properties

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int RideRequestId { get; set; }

        public int UserId { get; set; }

        #endregion
    }
}
