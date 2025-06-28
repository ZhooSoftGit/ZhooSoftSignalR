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
        public IActionResult GetDriverLocation(string driverId)
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
    }
}
