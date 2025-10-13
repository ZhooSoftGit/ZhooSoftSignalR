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
