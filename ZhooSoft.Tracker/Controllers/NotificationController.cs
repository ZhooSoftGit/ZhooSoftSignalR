using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ZhooSoft.Tracker.Hubs;
using ZhooSoft.Tracker.Models;
using ZhooSoft.Tracker.Services;

namespace ZhooSoft.Tracker.Controllers
{
    [ApiController]
    [Route("api/notify")]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<DriverLocationHub> _hubContext;

        public NotificationController(IHubContext<DriverLocationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("send-to-user")]
        public async Task<IActionResult> SendToUser([FromBody] MessageDto dto)
        {
            var connId = ConnectionMapping.GetConnection(dto.UserId);
            if (!string.IsNullOrEmpty(connId))
            {
                await _hubContext.Clients.Client(connId).SendAsync("ReceiveNotification", dto.Message);
            }

            return Ok();
        }
    }
}
