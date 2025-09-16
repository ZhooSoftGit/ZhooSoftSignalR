using System.Collections.Concurrent;
using ZhooSoft.Tracker.Models;

namespace ZhooSoft.Tracker.Store
{
    public static class RideConnectionMapping
    {
        // Use BookingRequestId as the key
        public static ConcurrentDictionary<int, RideConnectionInfo> ActiveRides = new();
    }
}
