using Zhoosoft.EventBus;

namespace ZhooSoft.Tracker.AzureServiceBus
{
    #region Interfaces

    public interface ITrackerApiRideEventPublisher
    {
        #region Methods

        Task PublishAsync(
            EventType eventType,
            int rideId,
            int userId,
            int? driverId = null,
            string? otp = null,
            object payload = null);

        Task PublishDriverOfferTimeoutEventAsync(int rideRequestId, int userId);

        #endregion
    }

    #endregion
}
