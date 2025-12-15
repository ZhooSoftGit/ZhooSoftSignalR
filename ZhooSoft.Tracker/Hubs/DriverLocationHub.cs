using Microsoft.AspNetCore.SignalR;
using ZhooSoft.Tracker.AzureServiceBus;
using ZhooSoft.Tracker.Models;

namespace ZhooSoft.Tracker.Hubs
{
    //[Authorize]
    public class DriverLocationHub : Hub
    {
        #region Fields

        private readonly BookingMonitorService _bookingMonitorService;

        private readonly ITrackerApiRideEventPublisher _publishEventHandler;

        private readonly DriverRedisRepository _redis;

        #endregion

        #region Constructors

        public DriverLocationHub(
            DriverRedisRepository redis,
            ITrackerApiRideEventPublisher publishEventHandler,
            BookingMonitorService bookingMonitorService)
        {
            _redis = redis;
            _publishEventHandler = publishEventHandler;
            _bookingMonitorService = bookingMonitorService;
        }

        #endregion

        #region Methods

        // --------------------------------------------------------
        // GET NEARBY DRIVERS
        // --------------------------------------------------------
        public async Task<List<DriverLocation>> GetNearbyDrivers(double lat, double lng)
            => await _redis.GetNearbyIdleDriversAsync(lat, lng, 5);

        // Get User Connection ID
        public async Task<string?> GetUserConnectionId(int? userId)
        {
            if (userId == null) return null;
            return await _redis.GetConnectionAsync(userId.Value);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId != null)
            {
                await _redis.SetConnectionAsync(userId.Value, Context.ConnectionId);
                await _redis.SetUserStateAsync(userId.Value, UserAppState.Foreground);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId != null)
            {
                await _redis.SetUserStateAsync(userId.Value, UserAppState.Background);
                await _redis.RemoveConnectionAsync(userId.Value);
            }

            await base.OnDisconnectedAsync(exception);
        }       

        // --------------------------------------------------------
        // DRIVER LOCATION UPDATE
        // --------------------------------------------------------
        public async Task UpdateDriverLocation(DriverLocation loc)
        {
            var driverId = GetUserId();
            if (driverId == null) return;

            loc.DriverId = driverId.Value;
            await _redis.SetDriverLocationAsync(loc);

            _ = NotifyDriverLocationOnRide(driverId.Value, loc);
        }


        // --------------------------------------------------------
        // SEND LIVE LOCATION TO USER
        // --------------------------------------------------------
        private async Task NotifyDriverLocationOnRide(int driverId, DriverLocation location)
        {
            try
            {
                if (await _redis.GetOnRideUserIdByDriverAsync(driverId) is int userId && userId > 0)
                {
                    var userConn = await _redis.GetConnectionAsync(userId);
                    if (userConn != null)
                    {
                        await Clients.Client(userConn).SendAsync("ReceiveDriverLocation", location);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception (logging mechanism not shown here)
            }
        }

        /// <summary>
        /// Send ride message from driver to user or user to driver
        /// </summary>
        /// <param name="rideRequestId"></param>
        /// <param name="message"></param>
        /// <param name="senderType"></param>
        /// <param name="senderId"></param>
        /// <returns></returns>
        public async Task SendRideMessage(int rideRequestId, string message, string senderType, int senderId)
        {
            var connectionInfo = await _redis.GetRideConnectionAsync(rideRequestId);
            if (connectionInfo != null && connectionInfo.UserId.HasValue && connectionInfo.DriverId.HasValue)
            {
                var rideMessage = new RideMessage
                {
                    RideRequestId = rideRequestId,
                    SenderId = senderId,
                    SenderType = senderType,
                    Message = message
                };

                // figure out the recipient
                string? recipientConn = null;

                if (senderType.Equals("driver", StringComparison.OrdinalIgnoreCase))
                {
                    recipientConn = await _redis.GetConnectionAsync(connectionInfo.UserId.Value);
                }
                else if (senderType.Equals("user", StringComparison.OrdinalIgnoreCase))
                {
                    recipientConn = await _redis.GetConnectionAsync(connectionInfo.DriverId.Value);
                }

                if (recipientConn != null)
                {
                    await Clients.Client(recipientConn).SendAsync("ReceiveRideMessage", rideMessage);
                }
            }
        }

        private int? GetUserId()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var currentUserId))
            {
                return currentUserId;
            }
            return null;
        }

        #endregion
    }
}
