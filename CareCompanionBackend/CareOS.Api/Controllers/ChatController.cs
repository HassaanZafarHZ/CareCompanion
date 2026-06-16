using CareOS.Api.DTOs;
using CareOS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareOS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // GET: api/Chat
        [HttpGet]
        public async Task<IActionResult> GetChats()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }
            var result = await _chatService.GetRecentChatsAsync(userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        // POST: api/Chat/send
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto request)
        {
            var senderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Ensure user is sending from their own account
            if (request.SenderId != senderId)
            {
                return Unauthorized(new { message = "You can only send messages from your own account" });
            }

            var result = await _chatService.SendMessageAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        // GET: api/Chat/conversation/{otherUserId}?page=1&pageSize=50
        [HttpGet("conversation/{otherUserId}")]
        public async Task<IActionResult> GetConversation(
            string otherUserId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _chatService.GetConversationAsync(userId, otherUserId, page, pageSize);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Chat/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _chatService.GetUnreadCountAsync(userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Chat/mark-read/{messageId}
        [HttpPost("mark-read/{messageId}")]
        public async Task<IActionResult> MarkAsRead(string messageId)
        {
            var result = await _chatService.MarkAsReadAsync(messageId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Chat/recent
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentChats()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _chatService.GetRecentChatsAsync(userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}