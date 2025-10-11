namespace ZhooSoft.Tracker.Models
{
    #region Enums

    public enum RideTypeEnum
    {
        Local = 0,
        Rental = 1,
        Outstation = 2
    }

    #endregion

    public class AcceptRideRequest
    {
        #region Properties

        public int DriverId { get; set; }

        public int RideRequestId { get; set; }

        public int? VehicleId { get; set; }

        #endregion
    }

    public class RideRequestDto
    {
        #region Properties

        public int RideRequestId { get; set; }

        public DateTime? DropDateTime { get; set; }

        public double DropOffLatitude { get; set; }

        public string DropOffLocation { get; set; } = null!;

        public double DropOffLongitude { get; set; }

        public double? EstimatedDistance { get; set; }

        public int? EstimatedDuration { get; set; }

        public double? EstimatedFare { get; set; }

        public DateTime PickupDateTime { get; set; }

        public double PickUpLatitude { get; set; }

        public string PickUpLocation { get; set; } = null!;

        public double PickUpLongitude { get; set; }

        public int? RentalHours { get; set; }

        public RideStatus RideStatus { get; set; }

        public RideTypeEnum RideType { get; set; }

        public int UserId { get; set; }

        public VehicleTypeEnum VehicleType { get; set; }

        #endregion
    }

    public class UpdateTripStatusDto
    {
        #region Properties

        public bool? ForceStart { get; set; }

        public string? OTP { get; set; }

        public int RideRequestId { get; set; }

        public RideStatus? RideStatus { get; set; }

        #endregion
    }
}
