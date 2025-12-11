using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ZhooSoft.Tracker.Hubs;
using ZhooSoft.Tracker.Models;
using ZhooSoft.Tracker.Services;

namespace ZhooSoft.Tracker.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/notify")]
    public class NotificationController : ControllerBase
    {
        #region Fields

        private readonly IHubContext<DriverLocationHub> _hubContext;

        #endregion

        #region Constructors

        public NotificationController(IHubContext<DriverLocationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        #endregion

        #region Methods

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

        #endregion
    }
}
