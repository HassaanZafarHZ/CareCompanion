using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Models;
using MongoDB.Driver;

namespace CareOS.Api.Services
{
    public class CallService : ICallService
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<Call> _calls;
        private readonly IMongoCollection<User> _users;

        public CallService(MongoDbContext context)
        {
            _context = context;
            _calls = _context.GetCollection<Call>("Calls");
            _users = _context.GetCollection<User>("Users");
        }

        // INITIATE CALL
        public async Task<ApiResponse<Call>> InitiateCallAsync(InitiateCallDto request)
        {
            try
            {
                // Get caller and receiver details
                var caller = await _users.Find(u => u.Id == request.CallerId).FirstOrDefaultAsync();
                var receiver = await _users.Find(u => u.Id == request.ReceiverId).FirstOrDefaultAsync();

                if (caller == null || receiver == null)
                {
                    return ApiResponse<Call>.ErrorResponse("User not found");
                }

                // Check if receiver already has an active call
                var activeCall = await _calls.Find(c =>
                    (c.CallerId == request.ReceiverId || c.ReceiverId == request.ReceiverId) &&
                    (c.Status == "INITIATED" || c.Status == "RINGING" || c.Status == "ACCEPTED")
                ).FirstOrDefaultAsync();

                if (activeCall != null)
                {
                    return ApiResponse<Call>.ErrorResponse("User is already in a call");
                }

                var call = new Call
                {
                    CallerId = request.CallerId,
                    CallerName = caller.FullName,
                    ReceiverId = request.ReceiverId,
                    ReceiverName = receiver.FullName,
                    CallType = request.CallType,
                    Status = "RINGING",
                    InitiatedAt = DateTime.UtcNow
                };

                await _calls.InsertOneAsync(call);

                // TODO: Send real-time notification via SignalR
                // await _hubContext.Clients.User(request.ReceiverId).SendAsync("IncomingCall", call);

                return ApiResponse<Call>.SuccessResponse(call, "Call initiated");
            }
            catch (Exception ex)
            {
                return ApiResponse<Call>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // ACCEPT CALL
        public async Task<ApiResponse<Call>> AcceptCallAsync(string callId)
        {
            try
            {
                var call = await _calls.Find(c => c.Id == callId).FirstOrDefaultAsync();

                if (call == null)
                {
                    return ApiResponse<Call>.ErrorResponse("Call not found");
                }

                if (call.Status != "RINGING")
                {
                    return ApiResponse<Call>.ErrorResponse("Call is not in ringing state");
                }

                var update = Builders<Call>.Update
                    .Set(c => c.Status, "ACCEPTED")
                    .Set(c => c.AcceptedAt, DateTime.UtcNow);

                await _calls.UpdateOneAsync(c => c.Id == callId, update);

                call.Status = "ACCEPTED";
                call.AcceptedAt = DateTime.UtcNow;

                // TODO: Notify caller via SignalR
                // await _hubContext.Clients.User(call.CallerId).SendAsync("CallAccepted", call);

                return ApiResponse<Call>.SuccessResponse(call, "Call accepted");
            }
            catch (Exception ex)
            {
                return ApiResponse<Call>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // DECLINE CALL
        public async Task<ApiResponse<Call>> DeclineCallAsync(string callId)
        {
            try
            {
                var call = await _calls.Find(c => c.Id == callId).FirstOrDefaultAsync();

                if (call == null)
                {
                    return ApiResponse<Call>.ErrorResponse("Call not found");
                }

                var update = Builders<Call>.Update
                    .Set(c => c.Status, "DECLINED")
                    .Set(c => c.EndedAt, DateTime.UtcNow)
                    .Set(c => c.EndReason, "RECEIVER_DECLINED");

                await _calls.UpdateOneAsync(c => c.Id == callId, update);

                call.Status = "DECLINED";
                call.EndedAt = DateTime.UtcNow;

                // TODO: Notify caller via SignalR
                // await _hubContext.Clients.User(call.CallerId).SendAsync("CallDeclined", call);

                return ApiResponse<Call>.SuccessResponse(call, "Call declined");
            }
            catch (Exception ex)
            {
                return ApiResponse<Call>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // END CALL
        public async Task<ApiResponse<Call>> EndCallAsync(EndCallDto request)
        {
            try
            {
                var call = await _calls.Find(c => c.Id == request.CallId).FirstOrDefaultAsync();

                if (call == null)
                {
                    return ApiResponse<Call>.ErrorResponse("Call not found");
                }

                int duration = 0;
                if (call.AcceptedAt.HasValue)
                {
                    duration = (int)(DateTime.UtcNow - call.AcceptedAt.Value).TotalSeconds;
                }

                var update = Builders<Call>.Update
                    .Set(c => c.Status, "ENDED")
                    .Set(c => c.EndedAt, DateTime.UtcNow)
                    .Set(c => c.DurationSeconds, duration)
                    .Set(c => c.EndReason, request.EndReason);

                await _calls.UpdateOneAsync(c => c.Id == request.CallId, update);

                call.Status = "ENDED";
                call.EndedAt = DateTime.UtcNow;
                call.DurationSeconds = duration;

                return ApiResponse<Call>.SuccessResponse(call, "Call ended");
            }
            catch (Exception ex)
            {
                return ApiResponse<Call>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET CALL HISTORY
        public async Task<ApiResponse<List<Call>>> GetCallHistoryAsync(string userId)
        {
            try
            {
                var calls = await _calls
                    .Find(c => c.CallerId == userId || c.ReceiverId == userId)
                    .SortByDescending(c => c.InitiatedAt)
                    .Limit(50)
                    .ToListAsync();

                return ApiResponse<List<Call>>.SuccessResponse(calls);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<Call>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET ACTIVE CALL
        public async Task<ApiResponse<Call>> GetActiveCallAsync(string userId)
        {
            try
            {
                var call = await _calls.Find(c =>
                    (c.CallerId == userId || c.ReceiverId == userId) &&
                    (c.Status == "RINGING" || c.Status == "ACCEPTED")
                ).FirstOrDefaultAsync();

                if (call == null)
                {
                    return ApiResponse<Call>.ErrorResponse("No active call");
                }

                return ApiResponse<Call>.SuccessResponse(call);
            }
            catch (Exception ex)
            {
                return ApiResponse<Call>.ErrorResponse($"Error: {ex.Message}");
            }
        }
    }
}