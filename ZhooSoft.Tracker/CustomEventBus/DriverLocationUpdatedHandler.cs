using Microsoft.AspNetCore.SignalR;
using ZhooSoft.Tracker.Hubs;
using ZhooSoft.Tracker.Services;
using ZhooSoft.Tracker.Store;

namespace ZhooSoft.Tracker.CustomEventBus
{
    public class DriverLocationUpdatedHandler : IIntegrationEventHandler<DriverLocationUpdatedEvent>
    {
        private readonly IHubContext<DriverLocationHub> _hubContext;

        public DriverLocationUpdatedHandler(IHubContext<DriverLocationHub> hubContext)
        {
            _hubContext = hubContext;
        }

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
                        new { @event.DriverId, @event.Latitude, @event.Longitude });
                }
            }
        }
    }
}
