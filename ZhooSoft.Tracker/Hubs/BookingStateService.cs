using System.Collections.Concurrent;
using ZhooSoft.Tracker.Models;

namespace ZhooSoft.Tracker.Hubs
{
    public class BookingStateService
    {
        #region Fields

        // Thread-safe dictionary to track pending bookings
        public ConcurrentDictionary<int, PendingBookingState> PendingBookings
            = new ConcurrentDictionary<int, PendingBookingState>();

        #endregion
    }
}
