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

    public class BookingRequestModel
    {
        #region Properties

        public int BookingRequestId { get; set; }

        public RideTypeEnum BookingType { get; set; }

        public string DistanceAndPayment { get; set; }

        public string DriverId { get; set; }

        public string DropAddress { get; set; }

        public double DropLatitude { get; set; }

        public double DropLongitude { get; set; }

        public string Fare { get; set; }

        public string PickupAddress { get; set; }

        public double PickupLatitude { get; set; }

        public double PickupLongitude { get; set; }

        public string PickupTime { get; set; }

        public int RemainingBids { get; set; }

        public int UserId { get; set; }

        public string? UserName { get; set; }

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
