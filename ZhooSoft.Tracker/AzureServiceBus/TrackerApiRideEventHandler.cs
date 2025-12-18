using Microsoft.AspNetCore.SignalR;
using ZCars.Abstractions;
using Zhoosoft.EventBus;
using ZhooSoft.Tracker.AzureServiceBus;
using ZhooSoft.Tracker.Helpers;
using ZhooSoft.Tracker.Hubs;
using ZhooSoft.Tracker.Models;

namespace ZhooSoft.Tracker
{
    public class TrackerApiRideEventHandler : ITrackerApiRideEventHandler
    {
        #region Fields

        private readonly BookingMonitorService _bookingMonitorService;

        private readonly DriverRedisRepository _driverRedisRepository;

        private readonly IHubContext<DriverLocationHub> _hub;

        private readonly ITrackerApiRideEventPublisher _trackerApiRideEventPublisher;

        #endregion

        #region Constructors

        public TrackerApiRideEventHandler(
            IHubContext<DriverLocationHub> hub,
            DriverRedisRepository driverRedisRepository,
            ITrackerApiRideEventPublisher trackerApiRideEventPublisher,
            BookingMonitorService bookingMonitorService)
        {
            _hub = hub;
            _driverRedisRepository = driverRedisRepository;
            _trackerApiRideEventPublisher = trackerApiRideEventPublisher;
            _bookingMonitorService = bookingMonitorService;
        }

        #endregion

        #region Methods

        public async Task<string?> GetUserConnectionId(int? userId)
        {
            if (userId == null) return null;
            return await _driverRedisRepository.GetConnectionAsync(userId.Value);
        }

        public async Task HandleAsync(RideEventMessage msg)
        {
            switch (msg.EventType)
            {
                case EventType.RideRequested:
                    await HandleRideRequested(msg);
                    break;

                case EventType.RideConfirmation:
                    await HandleRideConfirmed(msg);
                    break;

                case EventType.StartPickupNotification or
                    EventType.PickupReachedNotification or
                    EventType.StartRideWithOTP or
                    EventType.EndRideWithOTP or
                    EventType.RideCancellation:

                    await HandleRideNotification(msg);
                    break;

                default:
                    Console.WriteLine($"[SignalR API] Unhandled Event: {msg.EventType}");
                    break;
            }
        }

        // SEND BOOKING REQUEST (TOP IDLE DRIVERS)
        // --------------------------------------------------------
        public async Task SendBookingRequest(RideRequestDto request)
        {
            var idleDrivers = await _driverRedisRepository.GetTopIdleDriversAsync(
                request.PickupLatitude.Value,
                request.PickupLongitude.Value,
                5
            );

            if (idleDrivers.Count == 0)
            {
                var conn = await _driverRedisRepository.GetConnectionAsync(request.UserId);
                if (conn != null)
                    await _hub.Clients.Client(conn).SendAsync("NoDriverAvailable", request.RideRequestId);

                await _trackerApiRideEventPublisher.PublishDriverOfferTimeoutEventAsync(request.RideRequestId, request.RideRequestId);

                return;
            }

            foreach (var driver in idleDrivers)
            {
                var conn = await _driverRedisRepository.GetConnectionAsync(driver.DriverId);
                if (conn != null)
                    await _hub.Clients.Client(conn).SendAsync("ReceiveBookingRequest", request);
            }

            await _driverRedisRepository.CreateRideHashAsync(request.RideRequestId, request.UserId);

            _ = _bookingMonitorService.MonitorBookingAsync(new PendingBookingState
            {
                UserId = request.UserId,
                RideRequestId = request.RideRequestId,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Ride notification
        public async Task SendRideConfirmationNotification(RideEventMessage eventMessage)
        {
            if (eventMessage.DriverId != null && eventMessage.RideRequestId != null)
            {
                await _driverRedisRepository.UpdateRideStatusAsync(eventMessage.RideRequestId.Value, eventMessage.DriverId.Value, RideStatus.Assigned);
                await _driverRedisRepository.SetDriverOnRideAsync(eventMessage.DriverId.Value, eventMessage.RideRequestId.Value);
                var userConn = await GetUserConnectionId(eventMessage.UserId);
                if (userConn != null)
                {
                    await _hub.Clients.Client(userConn).SendAsync("BookingConfirmed", new RideEventModel
                    {
                        RideRequestId = eventMessage.RideRequestId.Value,
                        DriverId = eventMessage.DriverId.Value,
                        Status = RideStatus.Assigned,
                        UserId = eventMessage.UserId
                    });
                }
            }
        }

        public async Task SendRideStatusNotification(RideEventModel rideEventModel)
        {

            if (await GetUserConnectionId(rideEventModel.DriverId) is string driverConn)
            {
                if (driverConn != null)
                {
                    await _hub.Clients.Client(driverConn).SendAsync("OnTripNotification", rideEventModel);
                }
            }
            if (await GetUserConnectionId(rideEventModel.UserId) is string userConn)
            {
                if (userConn != null)
                {
                    await _hub.Clients.Client(userConn).SendAsync("OnTripNotification", rideEventModel);
                }
            }

            if (rideEventModel.Status == RideStatus.Cancelled || rideEventModel.Status == RideStatus.Completed)
            {
                await _driverRedisRepository.ClearDriverOnRideAsync(rideEventModel.DriverId);
                await _driverRedisRepository.ClearRideInfoAsync(rideEventModel.RideRequestId);
            }
        }

        private async Task HandleRideConfirmed(RideEventMessage msg)
        {
            await SendRideConfirmationNotification(msg);
            Console.WriteLine("[SignalR] Sent RideConfirmation to user and driver");
        }

        private async Task HandleRideNotification(RideEventMessage msg)
        {
            RideStatus? type = null;

            switch (msg.EventType)
            {
                case EventType.StartPickupNotification:
                    type = RideStatus.StartedToPickup;
                    break;
                case EventType.PickupReachedNotification:
                    type = RideStatus.Reached;
                    break;
                case EventType.StartRideWithOTP:
                    type = RideStatus.Started;
                    break;
                case EventType.EndRideWithOTP:
                    type = RideStatus.Completed;
                    break;
                case EventType.RideCancellation:
                    type = RideStatus.Cancelled;
                    break;
            }

            if (msg.DriverId == null || msg.RideRequestId == null || type == null)
            {
                Console.WriteLine("[SignalR] Invalid Ride Notification Message");
                return;
            }
            await SendRideStatusNotification(new RideEventModel { DriverId = msg.DriverId.Value, UserId = msg.UserId, RideRequestId = msg.RideRequestId.Value, Status = type.Value });
        }

        private async Task HandleRideRequested(RideEventMessage msg)
        {
            if (msg.Payload != null)
            {
                var payload = PayloadConverter.ConvertPayload<RideRequestDto>(msg.Payload);

                if (payload != null)
                {
                    await SendBookingRequest(payload);
                }
            }
            Console.WriteLine("[SignalR] Sent RideRequested to drivers");
        }

        #endregion
    }
}
