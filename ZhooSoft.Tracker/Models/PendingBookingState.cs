namespace ZhooSoft.Tracker.Models
{
    public class PendingBookingState
    {
        #region Properties

        public int? AssignedDriverId { get; set; }

        public TaskCompletionSource<int> AssignmentTcs { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int RideRequestId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public object LockObj { get; } = new();

        public HashSet<int> OfferedDrivers { get; set; } = new();

        public HashSet<int> RespondedDrivers { get; set; } = new();

        public CancellationTokenSource TimeoutCts { get; set; } = new();

        public int UserId { get; set; }

        #endregion
    }
}
