namespace ZhooSoft.Tracker.Models
{
    public enum VehicleTypeEnum
    {
        Hatchback = 1,      // Compact small cars
        Sedan,          // Medium-sized cars with a separate trunk
        SUV,            // Sport Utility Vehicles
        MPV,            // Multi-Purpose Vehicles
        EV,             // Electric Vehicles
        Luxury,         // High-end luxury cars
        AutoRickshaw,   // Three-wheeler city ride
        BikeTaxi
    }


    public enum RideStatus
    {
        Requested,       // Ride just requested
        Scheduled,       // Ride is scheduled for a future time        
        Assigned,        // Driver assigned
        StartedToPickup, // Pickup
        Reached,         // Driver reached pickup point
        Started,         // Ride in progress
        Completed,       // Ride completed successfully
        Cancelled,       // Ride cancelled by user or driver
        Failed,          // System/payment/route failure
        NoDrivers,      // No driver accepted in time
        Rejected         // Driver explicitly rejected
    }
}
