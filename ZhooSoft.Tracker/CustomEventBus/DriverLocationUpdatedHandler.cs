using Microsoft.AspNetCore.SignalR;
using ZhooSoft.Tracker.Hubs;
using ZhooSoft.Tracker.Models;
using ZhooSoft.Tracker.Services;
using ZhooSoft.Tracker.Store;

namespace ZhooSoft.Tracker.CustomEventBus
{
    public class DriverLocationUpdatedHandler : IIntegrationEventHandler<DriverLocationUpdatedEvent>
    {
        #region Fields

        private readonly IHubContext<DriverLocationHub> _hubContext;

        #endregion

        #region Constructors

        public DriverLocationUpdatedHandler(IHubContext<DriverLocationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        #endregion

        #region Methods

        public async Task HandleAsync(DriverLocationUpdatedEvent @event)
        {
            var entry = RideConnectionMapping.ActiveRides
                .FirstOrDefault(kvp => kvp.Value.DriverId == @event.DriverId);

            if (entry.Value != null)
            {
                var userConn = ConnectionMapping.GetConnection(entry.Value.UserId);
                if (userConn != null)
                {
                    await _hubContext.Clients.Client(userConn).SendAsync(
                        "ReceiveDriverLocation",
                        new DriverLocation { DriverId = @event.DriverId, Latitude = @event.Latitude, Longitude = @event.Longitude });
                }
            }
        }

        #endregion
    }
}
