using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ZhooSoft.Tracker.CustomEventBus;
using ZhooSoft.Tracker.Models;
using ZhooSoft.Tracker.Services;
using ZhooSoft.Tracker.Store;

namespace ZhooSoft.Tracker.Hubs
{
    [Authorize]
    public class DriverLocationHub : Hub
    {
        private readonly DriverLocationStore _store;
        private BookingMonitorService _bookingMonitorService;
        private BookingStateService _bookingStateService;
        private IEventBus _eventBus;
        private IMainApiService _mainApiService;

        public DriverLocationHub(DriverLocationStore store, BookingMonitorService bookingMonitorService,
            BookingStateService stateService, IEventBus eventBus, IMainApiService mainApiService)
        {
            _store = store;
            _bookingMonitorService = bookingMonitorService;
            _bookingStateService = stateService;
            _eventBus = eventBus;
            _mainApiService = mainApiService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId != null)
                ConnectionMapping.Add(userId.Value, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            var userId = GetUserId();
            if (userId != null)
            {
                ConnectionMapping.Remove(userId.Value);
                _store.Remove(userId.Value);
            }

            await base.OnDisconnectedAsync(ex);
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

        public async Task UpdateDriverLocation(DriverLocation location)
        {
            var driverId = GetUserId();
            if (driverId != null)
            {
                location.DriverId = driverId.Value;
                _store.Update(driverId.Value, location);

                await _eventBus.PublishAsync(new DriverLocationUpdatedEvent(driverId.Value, location.Latitude, location.Longitude));
            }
        }

        public Task<List<DriverLocation>> GetNearbyDrivers(double lat, double lng)
        {
            var result = _store.GetNearby(lat, lng, 5); // 5 KM radius
            return Task.FromResult(result);
        }


        public async Task SendBookingRequest(RideRequestDto booking)
        {
            if (!IsRequestValid(booking))
            {
                var userConn = ConnectionMapping.GetConnection(booking.UserId);
                if (userConn != null)
                {
                    await Clients.Client(userConn).SendAsync("NoDriverAvailable", booking.RideRequestId);
                }
                return;
            }

            var nearby = _store.GetNearbyIdleDriver(booking.PickUpLatitude, booking.PickUpLongitude, 5);
            var activeDriverIds = RideConnectionMapping.ActiveRides.Values.Select(r => r.DriverId).ToHashSet();
            var IdleDriver = nearby.Where(d => !activeDriverIds.Contains(d.DriverId)).ToList();
            if (IdleDriver.Count == 0)
            {
                var userConn = ConnectionMapping.GetConnection(booking.UserId);
                if (userConn != null)
                {
                    _ = _mainApiService.UpdateBookingStatus(new UpdateTripStatusDto { RideRequestId = booking.RideRequestId, RideStatus = RideStatus.NoDrivers });
                    await Clients.Client(userConn).SendAsync("NoDriverAvailable", booking.RideRequestId);
                }
                return;
            }

            var bookingState = new PendingBookingState
            {
                RideRequestId = booking.RideRequestId, // int
                UserId = booking.UserId
            };

            var driversToOffer = IdleDriver.Take(5).ToList();

            foreach (var driver in driversToOffer)
            {
                bookingState.OfferedDrivers.Add(driver.DriverId);

                var driverConn = ConnectionMapping.GetConnection(driver.DriverId);
                if (driverConn != null)
                {
                    await Clients.Client(driverConn).SendAsync("ReceiveBookingRequest", booking);
                }
            }

            _bookingStateService.PendingBookings[bookingState.RideRequestId] = bookingState;

            // start monitor task
            _ = _bookingMonitorService.MonitorBookingAsync(bookingState);
        }

        private bool IsRequestValid(RideRequestDto booking)
        {
            bool hasPending = _bookingStateService.PendingBookings.Values
                                .Any(b => b.UserId == booking.UserId);

            if (hasPending)
            {
                return false;
            }

            // 2. Check in ActiveRides (in-memory/redis)
            if (RideConnectionMapping.ActiveRides.Values.Any(r => r.UserId == booking.UserId))
                return false;

            return true;
        }

        public async Task RespondToBookingByDriver(int userId, int driverId, int rideRequestId, string status)
        {
            if (!_bookingStateService.PendingBookings.TryGetValue(rideRequestId, out var bookingState))
            {
                await Clients.Caller.SendAsync("BookingExpired", rideRequestId);
                return;
            }

            if (bookingState.AssignedDriverId != null || bookingState.RespondedDrivers.Contains(driverId))
                return;

            bookingState.RespondedDrivers.Add(driverId);

            if (status.Equals("assigned", StringComparison.OrdinalIgnoreCase))
            {
                bookingState.AssignedDriverId = driverId;
                bookingState.AssignmentTcs.TrySetResult(driverId);
            }
            else if (bookingState.RespondedDrivers.Count >= 5)
            {
                bookingState.AssignmentTcs.TrySetCanceled();
            }
        }

        public async Task<bool> StartRide(int rideRequestId, string enteredOtp)
        {
            if (RideConnectionMapping.ActiveRides.TryGetValue(rideRequestId, out var ride))
            {
                if (enteredOtp == ride.StartTripOtp)
                {
                    var userConn = ConnectionMapping.GetConnection(ride.UserId);
                    if (userConn != null)
                        await Clients.Client(userConn).SendAsync("TripStarted", rideRequestId);

                    var driverConn = ConnectionMapping.GetConnection(ride.DriverId);
                    if (driverConn != null)
                        await Clients.Client(driverConn).SendAsync("TripStarted", rideRequestId);

                    return true;
                }
                else
                {
                    await Clients.Caller.SendAsync("OtpVerificationFailed", new
                    {
                        Reason = "Invalid Start OTP"
                    });
                    return false;
                }
            }

            return false;
        }

        public async Task<bool> EndRide(int rideRequestId, string enteredOtp)
        {
            if (RideConnectionMapping.ActiveRides.TryGetValue(rideRequestId, out var ride))
            {
                if (enteredOtp == ride.EndTripOtp)
                {
                    var userConn = ConnectionMapping.GetConnection(ride.UserId);
                    if (userConn != null)
                        await Clients.Client(userConn).SendAsync("TripCompleted", rideRequestId);

                    var driverConn = ConnectionMapping.GetConnection(ride.DriverId);
                    if (driverConn != null)
                        await Clients.Client(driverConn).SendAsync("TripCompleted", rideRequestId);

                    // Remove from active rides after completion
                    RideConnectionMapping.ActiveRides.TryRemove(rideRequestId, out _);

                    return true;
                }
                else
                {
                    await Clients.Caller.SendAsync("OtpVerificationFailed", new
                    {
                        Reason = "Invalid End OTP"
                    });
                    return false;
                }
            }

            return false;
        }



        public async Task SendLiveLocationToUser(int driverId)
        {
            var location = _store.Get(driverId);
            if (location != null)
            {
                await Clients.Caller.SendAsync("ReceiveDriverLocation", location);
            }
        }

        public async Task StartPickupNotification(int rideRequestId)
        {
            if (RideConnectionMapping.ActiveRides.TryGetValue(rideRequestId, out var rideInfo))
            {
                var userConn = ConnectionMapping.GetConnection(rideInfo.UserId);
                if (userConn != null)
                {
                    await Clients.Client(userConn).SendAsync("StartPickupNotification", rideRequestId);
                }
            }
        }

        public async Task PickupReachedNotification(int rideRequestId)
        {
            if (RideConnectionMapping.ActiveRides.TryGetValue(rideRequestId, out var participants))
            {
                var userConn = ConnectionMapping.GetConnection(participants.UserId);
                if (userConn != null)
                {
                    await Clients.Client(userConn).SendAsync("PickupReachedNotification", rideRequestId);
                }
            }
        }

        public async Task CancelBooking(int rideRequestId)
        {
            await CancelRideBooking(rideRequestId);
        }

        public async Task CancelTripNotification(int rideRequestId)
        {
            await CancelRideBooking(rideRequestId);
        }

        private async Task CancelRideBooking(int rideRequestId)
        {
            if (_bookingStateService.PendingBookings.TryRemove(rideRequestId, out var state))
            {
                // Cancel timeout task
                state.TimeoutCts.Cancel();

                // Cancel any pending assignment
                state.AssignmentTcs.TrySetCanceled();

                // Notify offered drivers
                foreach (var driverId in state.OfferedDrivers)
                {
                    var conn = ConnectionMapping.GetConnection(driverId);
                    if (conn != null)
                        await Clients.Client(conn)
                            .SendAsync("BookingCancelled", rideRequestId);
                }

                // Notify user (optional confirmation)
                var userConn = ConnectionMapping.GetConnection(state.UserId);
                if (userConn != null)
                    await Clients.Client(userConn)
                        .SendAsync("BookingCancelled", rideRequestId);
            }
            else if (RideConnectionMapping.ActiveRides.TryGetValue(rideRequestId, out var ride))
            {
                var userConn = ConnectionMapping.GetConnection(ride.UserId);
                if (userConn != null)
                    await Clients.Client(userConn).SendAsync("TripCancelled", rideRequestId);

                var driverConn = ConnectionMapping.GetConnection(ride.DriverId);
                if (driverConn != null)
                    await Clients.Client(driverConn).SendAsync("TripCancelled", rideRequestId);

                RideConnectionMapping.ActiveRides.TryRemove(rideRequestId, out _);
            }
        }


        public async Task SendRideMessage(int rideRequestId, string message, string senderType, int senderId)
        {
            if (RideConnectionMapping.ActiveRides.TryGetValue(rideRequestId, out var ride))
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
                    recipientConn = ConnectionMapping.GetConnection(ride.UserId);
                }
                else if (senderType.Equals("user", StringComparison.OrdinalIgnoreCase))
                {
                    recipientConn = ConnectionMapping.GetConnection(ride.DriverId);
                }

                if (recipientConn != null)
                {
                    await Clients.Client(recipientConn).SendAsync("ReceiveRideMessage", rideMessage);
                }
            }
        }

    }
}
