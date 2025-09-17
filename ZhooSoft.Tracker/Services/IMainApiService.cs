using ZhooSoft.Tracker.Models;

namespace ZhooSoft.Tracker.Services
{
    public interface IMainApiService
    {
        Task<RideTripDto> CreateRideAsync(int rideRequestId, int driverId);
        Task<bool> StartRideAsync(int rideId, string otp);
        Task<bool> EndRideAsync(int rideId, string otp);
    }
}
