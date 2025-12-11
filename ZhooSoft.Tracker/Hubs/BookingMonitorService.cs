using Microsoft.AspNetCore.SignalR;
using ZhooSoft.Tracker.AzureServiceBus;
using ZhooSoft.Tracker.Models;

namespace ZhooSoft.Tracker.Hubs
{
    public class BookingMonitorService
    {
        #region Fields

        private readonly IHubContext<DriverLocationHub> _hubContext;

        private BookingStateService _bookingStateService;

        private DriverRedisRepository _driverRedisRepository;

        private ITrackerApiRideEventPublisher _publishEventHandler;

        #endregion

        #region Constructors

        public BookingMonitorService(IHubContext<DriverLocationHub> hubContext,
            ITrackerApiRideEventPublisher publishEventHandler, DriverRedisRepository driverRedisRepository)
        {
            _hubContext = hubContext;
            _publishEventHandler = publishEventHandler;
            _driverRedisRepository = driverRedisRepository;
        }

        #endregion

        #region Methods

        public async Task<string?> GetUserConnectionId(int? userId)
        {
            if (userId == null) return null;
            return await _driverRedisRepository.GetConnectionAsync(userId.Value);
        }

        public async Task MonitorBookingAsync(PendingBookingState state)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(15));

                var status = await _driverRedisRepository.GetRideStatusAsync(state.RideRequestId);

                if (status == RideStatus.Requested)
                {
                    await _driverRedisRepository.UpdateRideStatusAsync(state.RideRequestId, RideStatus.NoDrivers);

                    // send no driver found response
                    await SendNoDriverNotification(new RideEventModel
                    {
                        RideRequestId = state.RideRequestId,
                        UserId = state.UserId
                    });
                    await _publishEventHandler.PublishDriverOfferTimeoutEventAsync(state.RideRequestId, state.UserId);
                }
            }
            catch (Exception ex)
            {
                //log the exception
            }
            finally
            {

            }
        }

        public async Task SendNoDriverNotification(RideEventModel rideEventModel)
        {
            if (await GetUserConnectionId(rideEventModel.UserId) is string userConn)
            {
                if (userConn != null)
                {
                    await _hubContext.Clients.Client(userConn).SendAsync("NoDriverAvailable", rideEventModel.RideRequestId);
                }
            }
        }

        #endregion
    }
}
