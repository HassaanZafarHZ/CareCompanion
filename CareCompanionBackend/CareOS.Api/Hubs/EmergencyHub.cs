using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CareOS.Api.Hubs
{
    [Authorize]
    public class EmergencyHub : Hub
    {
        // Connection established
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.Identity?.Name ?? "Unknown";

            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their personal group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

                Console.WriteLine($"✅ User connected: {userName} (ID: {userId})");
            }

            await base.OnConnectedAsync();
        }

        // Connection closed
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                Console.WriteLine($"❌ User disconnected: {userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // SEND EMERGENCY ALERT
        public async Task SendEmergencyAlert(string caretakerId, string message, string alertType)
        {
            await Clients.Group($"user_{caretakerId}").SendAsync("ReceiveEmergencyAlert", new
            {
                message,
                alertType,
                timestamp = DateTime.UtcNow
            });
        }

        // ACKNOWLEDGE EMERGENCY
        public async Task AcknowledgeEmergency(string elderId, string caretakerName)
        {
            await Clients.Group($"user_{elderId}").SendAsync("EmergencyAcknowledged", new
            {
                caretakerName,
                timestamp = DateTime.UtcNow
            });
        }
    }
}