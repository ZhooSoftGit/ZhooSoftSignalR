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

                var userConn = ConnectionMapping.GetConnection(state.UserId);


                if (finished == timeoutTask && userConn != null)
                {
                    if (timeoutTask.IsCanceled || state.AssignmentTcs.Task.IsCanceled)
                    {
                        return;
                    }

                    _ = _mainApiService.UpdateBookingStatus(new UpdateTripStatusDto { RideRequestId = state.RideRequestId, RideStatus = RideStatus.NoDrivers });

                    // timeout
                    await _hubContext.Clients.Client(userConn)
                        .SendAsync("NoDriverAvailable", state.RideRequestId);
                }
                else if (state.AssignmentTcs.Task.IsCompletedSuccessfully)
                {
                    var driverId = state.AssignmentTcs.Task.Result;
                    var driverConn = ConnectionMapping.GetConnection(driverId);

                    if (userConn != null)
                    {
                        var ride = await _mainApiService.CreateRideAsync(new AcceptRideRequest { DriverId = driverId, RideRequestId = state.RideRequestId });

                        if (ride != null)
                        {
                            var rideInfo = new RideConnectionInfo
                            {
                                RideRequestId = state.RideRequestId,
                                UserId = state.UserId,
                                DriverId = driverId,
                                StartTripOtp = ride.StartOtp,
                                EndTripOtp = ride.EndOtp
                            };

                            RideConnectionMapping.ActiveRides[state.RideRequestId] = rideInfo;

                            await _hubContext.Clients.Client(userConn).SendAsync("BookingConfirmed", ride);

                            if (driverConn != null)
                            {
                                await _hubContext.Clients.Client(driverConn).SendAsync("BookingConfirmed", ride);
                            }
                        }
                        else if (driverConn != null)
                        {
                            await _hubContext.Clients.Client(driverConn).SendAsync("BookingCancelled", ride);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //log the exception
            }
            finally
            {
                state.TimeoutCts.Cancel();
                _bookingStateService.PendingBookings.TryRemove(state.RideRequestId, out _);
            }
        }
    }
}
