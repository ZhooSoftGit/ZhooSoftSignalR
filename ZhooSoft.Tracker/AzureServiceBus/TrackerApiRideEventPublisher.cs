using Zhoosoft.EventBus;

namespace ZhooSoft.Tracker.AzureServiceBus
{
    public class TrackerApiRideEventPublisher : ITrackerApiRideEventPublisher
    {
        #region Fields

        private readonly IEventPublisher _publisher;

        #endregion

        #region Constructors

        public TrackerApiRideEventPublisher(IEventPublisher publisher)
        {
            _publisher = publisher;
        }

        #endregion

        #region Methods

        // MAIN generic publish method
        public async Task PublishAsync(
            EventType eventType,
            int rideId,
            int userId,
            int? driverId = null,
            string? otp = null,
            object payload = null)
        {
            var message = new RideEventMessage
            {
                EventType = eventType,
                RideRequestId = rideId,
                UserId = userId,
                DriverId = driverId,
                OTP = otp,
                Payload = payload
            };

            await _publisher.PublishAsync(message);
        }

        public async Task PublishDriverOfferTimeoutEventAsync(int rideRequestId, int userId)
        {
            // 2. Publish event to Azure Service Bus
            var evt = new RideEventMessage
            {
                EventType = EventType.NoDriversAvailable,
                RideRequestId = rideRequestId,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            await _publisher.PublishAsync(evt);
        }

        #endregion
    }
}
