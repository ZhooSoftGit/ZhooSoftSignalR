using ZhooSoft.Tracker.Common;

namespace ZhooSoft.Tracker.Models
{
    public class DriverLocation
    {
        public string DriverId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DriverStatus? Status { get; set; }
    }
}
