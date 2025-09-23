namespace ZhooSoft.Tracker.CustomEventBus
{
    public class DriverLocationUpdatedEvent : IIntegrationEvent
    {
        public int DriverId { get; }
        public double Latitude { get; }
        public double Longitude { get; }

        public DriverLocationUpdatedEvent(int driverId, double latitude, double longitude)
        {
            DriverId = driverId;
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
