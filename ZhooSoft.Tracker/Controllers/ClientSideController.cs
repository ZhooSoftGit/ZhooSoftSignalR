using Microsoft.AspNetCore.Mvc;
using ZhooSoft.Tracker.Common;
using ZhooSoft.Tracker.Helpers;
using ZhooSoft.Tracker.Models;
using ZhooSoft.Tracker.Store;

namespace ZhooSoft.Tracker.Controllers
{
    [ApiController]
    [Route("api/clientside")]
    public class ClientSideController : ControllerBase
    {
        private readonly DriverLocationStore _store;
        private readonly ILogger<ClientSideController> _logger;

        public ClientSideController(DriverLocationStore store, ILogger<ClientSideController> logger)
        {
            _store = store;
            _logger = logger;
        }

        [HttpGet("nearby-drivers")]
        [ClientAuth]
        public IActionResult GetNearbyDrivers([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] double radiusKm = 5)
        {
            var nearbyDrivers = _store.GetNearby(latitude,longitude,radiusKm).ToList();
            return Ok(nearbyDrivers);
        }
    }
}
