namespace ZhooSoft.Tracker.Models
{
    public class RideConnectionInfo
    {
        #region Properties

        public int DriverId { get; set; }

        public string EndTripOtp { get; set; } = default!;

        public int RideRequestId { get; set; }

        public string StartTripOtp { get; set; } = default!;

        public int UserId { get; set; }

        #endregion
    }
}
