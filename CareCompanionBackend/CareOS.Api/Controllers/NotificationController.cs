using CareOS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareOS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: api/Notification/my-notifications
        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _notificationService.GetUserNotificationsAsync(userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Notification/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _notificationService.GetUnreadCountAsync(userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Notification/mark-read/{id}
        [HttpPost("mark-read/{id}")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Notification/mark-all-read
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _notificationService.MarkAllAsReadAsync(userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // DELETE: api/Notification/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            var result = await _notificationService.DeleteNotificationAsync(id);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}