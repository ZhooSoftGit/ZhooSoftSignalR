using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using ZhooSoft.Tracker.Common;
using ZhooSoft.Tracker.Models;
using ZhooSoft.Tracker.Store;

namespace ZhooSoft.Tracker.Controllers
{
    [ApiController]
    [Route("api/internal")]
    public class InternalController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly DriverLocationStore _store;

        public InternalController(IConfiguration config, DriverLocationStore store)
        {
            _config = config;
            _store = store;
        }

        [HttpGet("driver-location/{driverId}")]
        [ServiceAuth] // ✅ Shared secret header check
        public IActionResult GetDriverLocation(int driverId)
        {
            var location = _store.Get(driverId);
            if (location == null)
                return NotFound("Driver location not available");

            return Ok(new DriverLocation
            {
                DriverId = driverId,
                Latitude = location.Latitude,
                Longitude = location.Longitude
            });
        }

        [HttpPost("driver-locations")]
        [ServiceAuth] // ✅ Shared secret header check
        public IActionResult GetDriverLocations([FromBody] List<int> driverIds)
        {
            if (driverIds == null || !driverIds.Any())
                return BadRequest("Driver IDs are required.");

            var results = driverIds
                .Select(id => new
                {
                    DriverId = id,
                    Location = _store.Get(id)
                })
                .Where(x => x.Location != null)
                .Select(x => new DriverLocation
                {
                    DriverId = x.DriverId,
                    Latitude = x.Location.Latitude,
                    Longitude = x.Location.Longitude
                })
                .ToList();

            if (!results.Any())
                return NotFound("No driver locations available.");

            return Ok(results);
        }
    }
}
