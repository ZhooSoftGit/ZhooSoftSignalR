using ZhooSoft.Tracker.Models;

namespace ZhooSoft.Tracker.Services
{
    #region Interfaces

    public interface IMainApiService
    {
        #region Methods

        Task<RideTripDto> CreateRideAsync(AcceptRideRequest rideRequest);

        Task<bool> EndRideAsync(int rideId, string otp);

        Task<bool> StartRideAsync(int rideId, string otp);

        Task<bool> UpdateBookingStatus(UpdateTripStatusDto rideRequest);

        #endregion
    }

    #endregion
}
