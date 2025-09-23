using ZhooSoft.Tracker.Models;

namespace ZhooSoft.Tracker.Services
{
    public interface IMainApiService
    {
        Task<RideTripDto> CreateRideAsync(AcceptRideRequest rideRequest);
        Task<bool> StartRideAsync(int rideId, string otp);
        Task<bool> EndRideAsync(int rideId, string otp);
    }
}
