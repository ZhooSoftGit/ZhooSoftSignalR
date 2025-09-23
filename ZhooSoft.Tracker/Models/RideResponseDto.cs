namespace ZhooSoft.Tracker.Models
{
    public class RideTripDto
    {
        #region Properties

        // Booking Info
        public int BookingRequestId { get; set; }
        public int UserId { get; set; }
        public string? PickupAddress { get; set; }
        public double? PickupLatitude { get; set; }
        public double? PickupLongitude { get; set; }
        public string? DropAddress { get; set; }
        public double? DropLatitude { get; set; }
        public double? DropLongitude { get; set; }
        public double? Fare { get; set; }
        public double? Distance { get; set; }
        public RideTypeEnum RideType { get; set; }

        // Driver Info
        public int? DriverId { get; set; }
        public string? DriverName { get; set; }
        public string? DriverPhoto { get; set; }
        public string? DriverPhone { get; set; }

        // Vehicle Info
        public int? VehicleId { get; set; }
        public string? VehicleNumber { get; set; }

        // Trip Status
        public RideStatus CurrentStatus { get; set; } = RideStatus.Requested;
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        // OTP Info
        public string? StartOtp { get; set; }
        public string? EndOtp { get; set; }
        #endregion
    }
}
