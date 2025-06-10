namespace ZhooSoft.Tracker.Models
{
    public class RideConnectionInfo
    {
        public string BookingRequestId { get; set; }
        public string UserId { get; set; }
        public string DriverId { get; set; }

        public string StartTripOtp { get; set; } = default!;
        public string EndTripOtp { get; set; } = default!;
    }
}
