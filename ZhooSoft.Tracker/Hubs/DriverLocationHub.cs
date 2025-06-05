using Microsoft.AspNetCore.SignalR;
using ZhooSoft.Tracker.Models;
using ZhooSoft.Tracker.Services;
using ZhooSoft.Tracker.Store;

namespace ZhooSoft.Tracker.Hubs
{
    //[Authorize]
    public class DriverLocationHub : Hub
    {
        private readonly DriverLocationStore _store;

        public DriverLocationHub(DriverLocationStore store)
        {
            _store = store;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId))
                ConnectionMapping.Add(userId, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                ConnectionMapping.Remove(userId);
                _store.Remove(userId);
            }

            await base.OnDisconnectedAsync(ex);
        }

        public Task UpdateDriverLocation(DriverLocation location)
        {
            var driverId = Context.GetHttpContext()?.Request.Query["userId"];
            if (!string.IsNullOrEmpty(driverId))
            {
                location.DriverId = driverId;
                location.Timestamp = DateTime.UtcNow;
                _store.Update(driverId, location);
            }
            return Task.CompletedTask;
        }

        public async Task GetNearbyDrivers(double lat, double lng)
        {
            var result = _store.GetNearby(lat, lng, 5); // 5 KM radius
            if (result.Count > 0)
            {
                await Clients.Caller.SendAsync("ReceiveNearbyDrivers", result);
            }
        }

        public async Task SendBookingRequest(BookingRequest booking)
        {
            var driverConn = ConnectionMapping.GetConnection(booking.DriverId);
            if (driverConn != null)
            {
                await Clients.Client(driverConn).SendAsync("ReceiveBookingRequest", booking);
            }
        }

        public async Task RespondToBookingByDriver(string userId, string driverId, string status)
        {
            var userConn = ConnectionMapping.GetConnection(userId);
            if (userConn != null)
            {
                await Clients.Client(userConn).SendAsync("BookingResponseFromDriver", new
                {
                    DriverId = driverId,
                    Status = status 
                });
            }
        }

        public async Task SendLiveLocationToUser(string userId)
        {
            var driverId = Context.GetHttpContext()?.Request.Query["userId"];
            if (string.IsNullOrEmpty(driverId)) return;

            var location = _store.Get(driverId);
            if (location != null)
            {
                var userConn = ConnectionMapping.GetConnection(userId);
                if (userConn != null)
                {
                    await Clients.Client(userConn).SendAsync("ReceiveDriverLocation", location);
                }
            }
        }
    }
}
