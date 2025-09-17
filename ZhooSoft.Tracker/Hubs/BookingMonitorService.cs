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
        private IMainApiService _mainApiService;

        public BookingMonitorService(IHubContext<DriverLocationHub> hubContext, BookingStateService stateService, IMainApiService mainApiService)
        {
            _hubContext = hubContext;
            _bookingStateService = stateService;
            _mainApiService = mainApiService;
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
                        var ride = await _mainApiService.CreateRideAsync(state.BookingRequestId, driverId);

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
