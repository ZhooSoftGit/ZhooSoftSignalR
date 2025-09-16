using System.Collections.Concurrent;
using ZhooSoft.Tracker.Models;

namespace ZhooSoft.Tracker.Store
{
    public class DriverLocationStore
    {
        private readonly ConcurrentDictionary<int, DriverLocation> _locations = new();

        public void Update(int driverId, DriverLocation location) => _locations[driverId] = location;

        public void Remove(int driverId) => _locations.TryRemove(driverId, out _);

        public List<DriverLocation> GetNearby(double lat, double lng, double radiusInKm)
        {
            return _locations.Values
                .Where(loc => GetDistance(lat, lng, loc.Latitude, loc.Longitude) <= radiusInKm)
                .ToList();
        }

        public List<DriverLocation> GetNearbyIdleDriver(double lat, double lng, double radiusInKm)
        {
            return _locations.Values
                .Where(loc => GetDistance(lat, lng, loc.Latitude, loc.Longitude) <= radiusInKm && loc.Status != Common.DriverStatus.InRide)
                .ToList();
        }

        public List<DriverLocation> GetAll()
        {
            return _locations.Values
                .ToList();
        }

        public DriverLocation? Get(int driverId) =>
            _locations.TryGetValue(driverId, out var loc) ? loc : null;

        private static double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
        }

        private static double DegreesToRadians(double deg) => deg * (Math.PI / 180);
    }
}
