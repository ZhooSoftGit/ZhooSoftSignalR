using System.Collections.Concurrent;
using ZhooSoft.Tracker.Models;

namespace ZhooSoft.Tracker.Store
{
    public static class RideConnectionMapping
    {
        #region Fields

        // Use RideRequestId as the key
        public static ConcurrentDictionary<int, RideConnectionInfo> ActiveRides = new();

        #endregion
    }
}
