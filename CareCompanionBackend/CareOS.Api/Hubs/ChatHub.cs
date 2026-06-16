using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CareOS.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                Console.WriteLine($"💬 Chat connected: {userId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // SEND MESSAGE
        public async Task SendMessage(string receiverId, string message, string messageType, string? detectedMood)
        {
            var senderId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var senderName = Context.User?.Identity?.Name ?? "Unknown";

            await Clients.Group($"user_{receiverId}").SendAsync("ReceiveMessage", new
            {
                senderId,
                senderName,
                message,
                messageType,
                detectedMood,
                timestamp = DateTime.UtcNow
            });
        }

        // TYPING INDICATOR
        public async Task UserTyping(string receiverId, bool isTyping)
        {
            var senderId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var senderName = Context.User?.Identity?.Name ?? "Unknown";

            await Clients.Group($"user_{receiverId}").SendAsync("UserTyping", new
            {
                senderId,
                senderName,
                isTyping
            });
        }
    }
}