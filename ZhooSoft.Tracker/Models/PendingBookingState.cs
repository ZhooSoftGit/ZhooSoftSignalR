namespace ZhooSoft.Tracker.Models
{
    public class PendingBookingState
    {
        public int BookingRequestId { get; set; }
        public int UserId { get; set; }
        public HashSet<int> OfferedDrivers { get; set; } = new();
        public HashSet<int> RespondedDrivers { get; set; } = new();
        public int? AssignedDriverId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public TaskCompletionSource<int> AssignmentTcs { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public CancellationTokenSource TimeoutCts { get; set; } = new();
        public object LockObj { get; } = new();
    }
}
