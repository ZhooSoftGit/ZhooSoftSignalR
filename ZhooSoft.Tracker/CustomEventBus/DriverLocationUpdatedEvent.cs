namespace ZhooSoft.Tracker.CustomEventBus
{
    public class DriverLocationUpdatedEvent : IIntegrationEvent
    {
        #region Constructors

        public DriverLocationUpdatedEvent(int driverId, double latitude, double longitude)
        {
            DriverId = driverId;
            Latitude = latitude;
            Longitude = longitude;
        }

        #endregion

        #region Properties

        public int DriverId { get; }

        public double Latitude { get; }

        public double Longitude { get; }

        #endregion
    }
}
