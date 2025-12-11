namespace ZhooSoft.Tracker.Models
{
    public class RideRequestDto
    {
        #region Properties

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public DateTime? DropoffDateTime { get; set; }

        public double? DropoffLatitude { get; set; }

        public string? DropoffLocation { get; set; }

        public double? DropoffLongitude { get; set; }

        public double? EstimatedDistance { get; set; }

        public int? EstimatedDuration { get; set; }

        public double? EstimatedFare { get; set; }

        public DateTime PickupDateTime { get; set; }

        public double? PickupLatitude { get; set; }

        public string? PickupLocation { get; set; }

        public double? PickupLongitude { get; set; }

        //Booking Type
        public int? RentalHours { get; set; }

        // Booking Info
        public int RideRequestId { get; set; }

        // Booking Status
        public RideStatus RideStatus { get; set; } = RideStatus.Requested;

        public RideTypeEnum RideType { get; set; }

        public int UserId { get; set; }

        public VehicleTypeEnum VehicleType { get; set; }

        #endregion
    }

    public class RideRequestModel
    {
        #region Properties

        public DateTime? DropoffDateTime { get; set; }

        public double DropoffLatitude { get; set; }

        public string DropoffLocation { get; set; } = null!;

        public double DropoffLongitude { get; set; }

        public double? EstimatedDistance { get; set; }

        public int? EstimatedDuration { get; set; }

        public double? EstimatedFare { get; set; }

        public DateTime PickupDateTime { get; set; }

        public double PickupLatitude { get; set; }

        public string PickupLocation { get; set; } = null!;

        public double PickupLongitude { get; set; }

        public int? RentalHours { get; set; }

        public RideStatus RideStatus { get; set; }

        public RideTypeEnum RideType { get; set; }

        public int UserId { get; set; }

        public VehicleTypeEnum VehicleType { get; set; }

        #endregion
    }

    public class RideTripDto : RideRequestDto
    {
        #region Properties

        // Driver Info
        public int? DriverId { get; set; }

        public string? DriverName { get; set; }

        public string? DriverPhone { get; set; }

        public string? DriverPhoto { get; set; }

        public string? EndOtp { get; set; }

        // OTP Info
        public string? StartOtp { get; set; }

        // Vehicle Info
        public int? VehicleId { get; set; }

        public string? VehicleNumber { get; set; }

        #endregion
    }
}
