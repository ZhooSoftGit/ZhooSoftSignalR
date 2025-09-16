using Microsoft.AspNetCore.SignalR;
using ZhooSoft.Tracker.Models;
using ZhooSoft.Tracker.Services;
using ZhooSoft.Tracker.Store;

namespace ZhooSoft.Tracker.Hubs
{
    public class BookingMonitorService
    {
        private readonly IHubContext<DriverLocationHub> _hubContext;

        private BookingStateService _bookingStateService;

        public BookingMonitorService(IHubContext<DriverLocationHub> hubContext, BookingStateService stateService)
        {
            _hubContext = hubContext;
            _bookingStateService = stateService;
        }

        public async Task MonitorBookingAsync(PendingBookingState state)
        {
            try
            {
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), state.TimeoutCts.Token);
                var finished = await Task.WhenAny(state.AssignmentTcs.Task, timeoutTask);

                if (finished == timeoutTask)
                {
                    // timeout
                    await _hubContext.Clients.Client(ConnectionMapping.GetConnection(state.UserId))
                        .SendAsync("NoDriverAvailable", state.BookingRequestId);

                    foreach (var driverId in state.OfferedDrivers)
                    {
                        var conn = ConnectionMapping.GetConnection(driverId);
                        if (conn != null)
                            await _hubContext.Clients.Client(conn).SendAsync("BookingExpired", state.BookingRequestId);
                    }
                }
                else if (state.AssignmentTcs.Task.IsCompletedSuccessfully)
                {
                    var driverId = state.AssignmentTcs.Task.Result;

                    var userConn = ConnectionMapping.GetConnection(state.UserId);
                    var driverConn = ConnectionMapping.GetConnection(driverId);

                    var startOtp = "1234"; // GenerateOtp() for testing
                    var endOtp = "1234";

                    var rideInfo = new RideConnectionInfo
                    {
                        BookingRequestId = state.BookingRequestId,
                        UserId = state.UserId,
                        DriverId = driverId,
                        StartTripOtp = startOtp,
                        EndTripOtp = endOtp
                    };

                    RideConnectionMapping.ActiveRides[state.BookingRequestId] = rideInfo;

                    if (userConn != null)
                    {
                        await _hubContext.Clients.Client(userConn).SendAsync("BookingConfirmed", new
                        {
                            DriverId = driverId,
                            Status = "assigned",
                            state.BookingRequestId,
                            StartOtp = startOtp,
                            EndOtp = endOtp
                        });
                    }

                    if (driverConn != null)
                    {
                        await _hubContext.Clients.Client(driverConn).SendAsync("BookingConfirmed", state.BookingRequestId);
                    }

                    // notify other drivers
                    foreach (var other in state.OfferedDrivers.Where(d => d != driverId))
                    {
                        var conn = ConnectionMapping.GetConnection(other);
                        if (conn != null)
                            await _hubContext.Clients.Client(conn).SendAsync("BookingExpired", state.BookingRequestId);
                    }
                }
            }
            catch(Exception ex)
            {

            }
            finally
            {
                state.TimeoutCts.Cancel();
                _bookingStateService.PendingBookings.TryRemove(state.BookingRequestId, out _);
            }
        }

        private static string GenerateOtp() => new Random().Next(1000, 9999).ToString();
    }
}
