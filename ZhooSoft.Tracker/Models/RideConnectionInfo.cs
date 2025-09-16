namespace ZhooSoft.Tracker.Models
{
    public class RideConnectionInfo
    {
        public int BookingRequestId { get; set; }
        public int UserId { get; set; }
        public int DriverId { get; set; }

        public string StartTripOtp { get; set; } = default!;
        public string EndTripOtp { get; set; } = default!;
    }
}
