namespace ZhooSoft.Tracker.Models
{
    public class DriverLocation
    {
        public string DriverId { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
