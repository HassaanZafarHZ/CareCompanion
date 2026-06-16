using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CareOS.Api.Hubs
{
    [Authorize]
    public class CallHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                Console.WriteLine($"📞 Call connected: {userId}");
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

        // INCOMING CALL NOTIFICATION
        public async Task NotifyIncomingCall(string receiverId, string callId, string callType, string callerName)
        {
            await Clients.Group($"user_{receiverId}").SendAsync("IncomingCall", new
            {
                callId,
                callType,
                callerName,
                timestamp = DateTime.UtcNow
            });
        }

        // CALL ACCEPTED
        public async Task NotifyCallAccepted(string callerId, string callId)
        {
            await Clients.Group($"user_{callerId}").SendAsync("CallAccepted", new
            {
                callId,
                timestamp = DateTime.UtcNow
            });
        }

        // CALL DECLINED
        public async Task NotifyCallDeclined(string callerId, string callId)
        {
            await Clients.Group($"user_{callerId}").SendAsync("CallDeclined", new
            {
                callId,
                timestamp = DateTime.UtcNow
            });
        }

        // CALL ENDED
        public async Task NotifyCallEnded(string otherUserId, string callId)
        {
            await Clients.Group($"user_{otherUserId}").SendAsync("CallEnded", new
            {
                callId,
                timestamp = DateTime.UtcNow
            });
        }

        // WEBRTC SIGNALING (for actual video/audio connection)
        public async Task SendOffer(string receiverId, string offer)
        {
            await Clients.Group($"user_{receiverId}").SendAsync("ReceiveOffer", offer);
        }

        public async Task SendAnswer(string callerId, string answer)
        {
            await Clients.Group($"user_{callerId}").SendAsync("ReceiveAnswer", answer);
        }

        public async Task SendIceCandidate(string otherUserId, string candidate)
        {
            await Clients.Group($"user_{otherUserId}").SendAsync("ReceiveIceCandidate", candidate);
        }
    }
}