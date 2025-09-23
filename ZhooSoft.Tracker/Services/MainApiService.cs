using ZhooSoft.Tracker.Models;

namespace ZhooSoft.Tracker.Services
{
    public class MainApiService : IMainApiService
    {
        private readonly HttpClient _client;

        public MainApiService(HttpClient client, IConfiguration config)
        {
            _client = client;
            _client.BaseAddress = new Uri(config["MainApi:BaseUrl"]);
            _client.DefaultRequestHeaders.Add("X-Service-Auth", config["ServiceAuth:SharedSecret"]!); // for now
        }

        public async Task<RideTripDto> CreateRideAsync(AcceptRideRequest rideRequest)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("api/taxi/accept-ride", rideRequest);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<RideTripDto>();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> StartRideAsync(int rideId, string otp)
        {
            var response = await _client.PostAsJsonAsync($"api/rides/{rideId}/start", new { otp });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EndRideAsync(int rideId, string otp)
        {
            var response = await _client.PostAsJsonAsync($"api/rides/{rideId}/end", new { otp });
            return response.IsSuccessStatusCode;
        }
    }
}
