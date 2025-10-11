namespace ZhooSoft.Tracker.Models
{
    public class RideConnectionInfo
    {
        public int RideRequestId { get; set; }
        public int UserId { get; set; }
        public int DriverId { get; set; }

        public string StartTripOtp { get; set; } = default!;
        public string EndTripOtp { get; set; } = default!;
    }
}
