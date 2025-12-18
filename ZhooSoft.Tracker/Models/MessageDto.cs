using ZCars.Abstractions;

namespace ZhooSoft.Tracker.Models
{
    public class MessageDto
    {
        #region Properties

        public string Message { get; set; }

        public int UserId { get; set; }

        #endregion
    }

    public class RideEventModel
    {
        #region Properties

        public int DriverId { get; set; }

        public object? Payload { get; set; }

        public int RideRequestId { get; set; }

        public RideStatus Status { get; set; }

        public int UserId { get; set; }

        #endregion
    }
}
