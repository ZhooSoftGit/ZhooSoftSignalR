namespace ZhooSoft.Tracker.Models
{
    public class RideTripDto
    {
        #region Properties

        public DateTime? CreatedAt { get; set; }

        public double? Distance { get; set; }

        public int DriverId { get; set; }

        public DateTime? EndTime { get; set; }

        public double? Fare { get; set; }

        public bool? IsActive { get; set; }

        public int RideRequestId { get; set; }

        public int RideTripId { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int VehicleId { get; set; }

        #endregion
    }
}
