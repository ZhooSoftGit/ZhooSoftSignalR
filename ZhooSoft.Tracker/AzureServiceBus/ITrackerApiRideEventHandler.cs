using Zhoosoft.EventBus;

namespace ZhooSoft.Tracker
{
    #region Interfaces

    public interface ITrackerApiRideEventHandler
    {
        #region Methods

        Task HandleAsync(RideEventMessage message);

        #endregion
    }

    #endregion
}
