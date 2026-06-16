using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CareOS.Api.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                Console.WriteLine($"🔔 Notification connected: {userId}");
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

        // SEND NOTIFICATION
        public async Task SendNotification(string userId, string title, string message, string type)
        {
            await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", new
            {
                title,
                message,
                type,
                timestamp = DateTime.UtcNow
            });
        }

        // MEDICINE REMINDER
        public async Task SendMedicineReminder(string elderId, string medicineName, string time)
        {
            await Clients.Group($"user_{elderId}").SendAsync("MedicineReminder", new
            {
                medicineName,
                time,
                timestamp = DateTime.UtcNow
            });
        }

        // DAILY CHECK-IN REMINDER
        public async Task SendCheckInReminder(string elderId)
        {
            await Clients.Group($"user_{elderId}").SendAsync("CheckInReminder", new
            {
                message = "Time for your daily wellness check-in",
                timestamp = DateTime.UtcNow
            });
        }
    }
}