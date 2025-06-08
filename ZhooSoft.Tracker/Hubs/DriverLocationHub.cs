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

        public async Task SendBookingRequest(BookingRequestModel booking)
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

        public async Task SendLiveLocationToUser(string driverId)
        {
            if (string.IsNullOrEmpty(driverId)) return;

            var location = _store.Get(driverId);
            if (location != null)
            {
                // Send back to the user who invoked this method
                await Clients.Caller.SendAsync("ReceiveDriverLocation", location);
            }
        }

        #region Dummy methods
        public async Task DummyTriggerforBookingRequest()
        {
            var driverId = Context.GetHttpContext()?.Request.Query["userId"];
            var model = new BookingRequestModel
            {
                BoookingRequestId = 1,
                BookingType = RideTypeEnum.Local,
                Fare = "₹ 194",
                DistanceAndPayment = "0.1 Km / Cash",
                PickupLocation = "Muthanampalayam, Tiruppur",
                PickupAddress = "3/21, Muthanampalayam, Tiruppur, Tamil Nadu 641606, India",
                PickupLatitude = 11.0176,
                PickupLongitude = 76.9674,
                PickupTime = "06 Feb 2024, 07:15 PM",
                DropoffLocation = "Tiruppur Old Bus Stand",
                DropLatitude = 10.9902,
                DropLongitude = 76.9629,
                RemainingBids = 3,
                DriverId = driverId,
                UserId = "0"
            };

            await SendBookingRequest(model);
        }


        #endregion
    }
}
