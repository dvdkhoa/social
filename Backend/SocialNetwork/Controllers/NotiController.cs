using Microsoft.AspNetCore.Mvc;
using SocialNetwork.BLL.Services;
using SocialNetwork.DTO.Entities;

namespace SocialNetwork.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotiController : ControllerBase
    {
        private readonly INotifyService _notifyService;

        public NotiController(INotifyService notifyService)
        {
            _notifyService = notifyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotification(string userId)
        {
            var notifications = await _notifyService.GetNotifycationByUserAsync(userId);

            var results = notifications.Select(notify => new
            {
                Id = notify.Id.ToString(),
                Message = notify.Message,
                Thumbnail = notify.Thumbnail,
                Type = notify.Type.ToString(),
                IntentId = notify.IntentId,
                Created = notify.Meta.Created,
                Seen = notify.Seen
            });

            return Ok(results);
        }
    }
}
