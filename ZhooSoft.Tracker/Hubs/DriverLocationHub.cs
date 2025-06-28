using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ZhooSoft.Tracker.Models;
using ZhooSoft.Tracker.Services;
using ZhooSoft.Tracker.Store;

namespace ZhooSoft.Tracker.Hubs
{
    [Authorize]
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
                _store.Update(driverId, location);

                // Send location to user if active ride exists
                var entry = RideConnectionMapping.ActiveRides.FirstOrDefault(kvp => kvp.Value.DriverId.Equals(driverId));
                if (!string.IsNullOrEmpty(entry.Key))
                {
                    var rideInfo = entry.Value;
                    var key = entry.Key;
                    var userconnection = ConnectionMapping.GetConnection(rideInfo.UserId);
                    if (userconnection != null)
                    {
                        Clients.Client(userconnection).SendAsync("ReceiveDriverLocation", new DriverLocation
                        {
                            DriverId = driverId,
                            Longitude = location.Longitude,
                            Latitude = location.Latitude
                        });
                    }

                }
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
                bool isDriverBusy = RideConnectionMapping.ActiveRides.Values
        .Any(ride => ride.DriverId == booking.DriverId);

                if (isDriverBusy)
                {
                    await Clients.Caller.SendAsync("BookingResponseFromDriver", new
                    {
                        booking.DriverId,
                        Status = "rejected"
                    });
                }
                else
                {
                    await Clients.Client(driverConn).SendAsync("ReceiveBookingRequest", booking);
                }
            }
        }

        public async Task RespondToBookingByDriver(string userId, string driverId, string bookingRequestId, string status)
        {
            var userConn = ConnectionMapping.GetConnection(userId);
            if (userConn != null)
            {
                await Clients.Client(userConn).SendAsync("BookingResponseFromDriver", new
                {
                    DriverId = driverId,
                    Status = status
                });

                if (status.Equals("assigned", StringComparison.OrdinalIgnoreCase))
                {
                    var startOtp = GenerateOtp();
                    var endOtp = GenerateOtp();

                    var rideInfo = new RideConnectionInfo
                    {
                        BookingRequestId = bookingRequestId,
                        UserId = userId,
                        DriverId = driverId,
                        StartTripOtp = startOtp,
                        EndTripOtp = endOtp
                    };

                    // Store ride connection using bookingRequestId as key
                    RideConnectionMapping.ActiveRides[bookingRequestId] = rideInfo;

                    await Clients.Client(userConn).SendAsync("ReceiveTripOtps", new
                    {
                        BookingRequestId = bookingRequestId,
                        StartOtp = rideInfo.StartTripOtp,
                        EndOtp = rideInfo.EndTripOtp
                    });
                }
            }
            else // FOr testing only
            {
                if (status.Equals("assigned", StringComparison.OrdinalIgnoreCase))
                {
                    var startOtp = GenerateOtp();
                    var endOtp = GenerateOtp();

                    var rideInfo = new RideConnectionInfo
                    {
                        BookingRequestId = bookingRequestId,
                        UserId = userId,
                        DriverId = driverId,
                        StartTripOtp = startOtp,
                        EndTripOtp = endOtp
                    };

                    // Store ride connection using bookingRequestId as key
                    RideConnectionMapping.ActiveRides[bookingRequestId] = rideInfo;
                }
            }
        }

        public async Task<bool> VerifyTripOtp(string bookingRequestId, string enteredOtp, bool isStart)
        {
            if (RideConnectionMapping.ActiveRides.TryGetValue(bookingRequestId, out var ride))
            {
                var expectedOtp = isStart ? ride.StartTripOtp : ride.EndTripOtp;

                if (enteredOtp == expectedOtp)
                {
                    string notificationMethod = isStart ? "TripStarted" : "TripCompleted";

                    var userConn = ConnectionMapping.GetConnection(ride.UserId);
                    if (userConn != null)
                    {
                        await Clients.Client(userConn).SendAsync(notificationMethod, bookingRequestId);
                    }
                    var driverConn = ConnectionMapping.GetConnection(ride.DriverId);
                    if (driverConn != null)
                    {
                        await Clients.Client(driverConn).SendAsync(notificationMethod, bookingRequestId);
                    }

                    // Clean up after end trip
                    if (!isStart)
                        RideConnectionMapping.ActiveRides.TryRemove(bookingRequestId, out _);

                    return true;
                }
                else
                {
                    await Clients.Caller.SendAsync("OtpVerificationFailed", new
                    {
                        Reason = isStart ? "Invalid Start OTP" : "Invalid End OTP"
                    });
                    return false;
                }
            }

            return false;
        }


        private static string GenerateOtp() => new Random().Next(1000, 9999).ToString();

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

        public async Task StartPickupNotification(string bookingRequestId)
        {
            if (RideConnectionMapping.ActiveRides.TryGetValue(bookingRequestId, out var rideInfo))
            {
                var userConn = ConnectionMapping.GetConnection(rideInfo.UserId);
                if (userConn != null)
                {
                    await Clients.Client(userConn).SendAsync("OnStartPickup", bookingRequestId);
                }
            }
        }

        public async Task PickupReachedNotification(string rideId)
        {
            if (RideConnectionMapping.ActiveRides.TryGetValue(rideId, out var participants))
            {
                await Clients.User(participants.UserId).SendAsync("OnPickupReached", rideId);
            }
        }

        public async Task CancelTripNotification(string rideId)
        {
            if (!RideConnectionMapping.ActiveRides.TryGetValue(rideId, out var participants))
                return;

            var userId = Context.GetHttpContext()?.Request.Query["userId"];
            var targetUserId = userId.Value == participants.UserId ? participants.DriverId : participants.UserId;

            var conn = ConnectionMapping.GetConnection(targetUserId);

            if (conn != null)
            {
                await Clients.Client(conn).SendAsync("OnTripCancelled", rideId);
            }

            // Remove from active rides
            RideConnectionMapping.ActiveRides.TryRemove(rideId, out _);
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
                UserId = "2"
            };

            await SendBookingRequest(model);
        }


        #endregion
    }
}
