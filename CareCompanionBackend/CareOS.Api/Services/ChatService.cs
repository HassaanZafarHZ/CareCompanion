using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Helpers;
using CareOS.Api.Models;
using MongoDB.Driver;


namespace CareOS.Api.Services
{
    public class ChatService : IChatService
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<ChatMessage> _messages;
        private readonly IMongoCollection<User> _users;
        private readonly IAiService _aiService;

        public ChatService(MongoDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
            _messages = _context.GetCollection<ChatMessage>("ChatMessages");
            _users = _context.GetCollection<User>("Users");
        }

        // SEND MESSAGE
        public async Task<ApiResponse<ChatMessage>> SendMessageAsync(SendMessageDto request)
        {
            try
            {
                // Get sender name
                var sender = await _users.Find(u => u.Id == request.SenderId).FirstOrDefaultAsync();
                if (sender == null)
                {
                    return ApiResponse<ChatMessage>.ErrorResponse("Sender not found");
                }

                // AI MOOD DETECTION (only for text messages from ELDER)
                string? detectedMood = null;
                if (request.MessageType == "TEXT" && sender.Role == "ELDER")
                {
                    var moodResult = await _aiService.DetectMoodFromTextAsync(request.MessageText);
                    if (moodResult.Success)
                    {
                        detectedMood = moodResult.Data;
                    }
                }

                var message = new ChatMessage
                {
                    SenderId = request.SenderId,
                    ReceiverId = request.ReceiverId,
                    SenderName = sender.FullName,
                    MessageText = request.MessageText,
                    MessageType = request.MessageType,
                    VoiceUrl = request.VoiceUrl,
                    DetectedMood = detectedMood,
                    IsRead = false,
                    SentAt = DateTime.UtcNow
                };

                await _messages.InsertOneAsync(message);

                // TODO: Send real-time notification via SignalR
                // await _hubContext.Clients.User(request.ReceiverId).SendAsync("NewMessage", message);

                return ApiResponse<ChatMessage>.SuccessResponse(message, "Message sent");
            }
            catch (Exception ex)
            {
                return ApiResponse<ChatMessage>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET CONVERSATION (with pagination)
        public async Task<ApiResponse<PaginatedResult<ChatResponseDto>>> GetConversationAsync(
            string userId,
            string otherUserId,
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                var filter = Builders<ChatMessage>.Filter.Or(
                    Builders<ChatMessage>.Filter.And(
                        Builders<ChatMessage>.Filter.Eq(m => m.SenderId, userId),
                        Builders<ChatMessage>.Filter.Eq(m => m.ReceiverId, otherUserId)
                    ),
                    Builders<ChatMessage>.Filter.And(
                        Builders<ChatMessage>.Filter.Eq(m => m.SenderId, otherUserId),
                        Builders<ChatMessage>.Filter.Eq(m => m.ReceiverId, userId)
                    )
                );

                var totalRecords = await _messages.CountDocumentsAsync(filter);

                var messages = await _messages
                    .Find(filter)
                    .SortByDescending(m => m.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                var response = messages.Select(m => new ChatResponseDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.SenderName,
                    MessageText = m.MessageText,
                    MessageType = m.MessageType,
                    VoiceUrl = m.VoiceUrl,
                    DetectedMood = m.DetectedMood,
                    IsRead = m.IsRead,
                    SentAt = m.SentAt
                }).Reverse().ToList(); // Reverse to show oldest first

                var paginatedResult = PaginationHelper.CreatePaginatedResult(
                    response,
                    page,
                    pageSize,
                    totalRecords
                );

                return ApiResponse<PaginatedResult<ChatResponseDto>>.SuccessResponse(paginatedResult);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResult<ChatResponseDto>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET UNREAD MESSAGE COUNT
        public async Task<ApiResponse<int>> GetUnreadCountAsync(string userId)
        {
            try
            {
                var count = await _messages.CountDocumentsAsync(m => m.ReceiverId == userId && !m.IsRead);

                return ApiResponse<int>.SuccessResponse((int)count);
            }
            catch (Exception ex)
            {
                return ApiResponse<int>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // MARK MESSAGE AS READ
        public async Task<ApiResponse<bool>> MarkAsReadAsync(string messageId)
        {
            try
            {
                var update = Builders<ChatMessage>.Update.Set(m => m.IsRead, true);
                var result = await _messages.UpdateOneAsync(m => m.Id == messageId, update);

                return ApiResponse<bool>.SuccessResponse(result.ModifiedCount > 0);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET RECENT CHATS (List of users you've chatted with)
        public async Task<ApiResponse<List<ChatResponseDto>>> GetRecentChatsAsync(string userId)
        {
            try
            {
                var recentMessages = await _messages
                    .Find(m => m.SenderId == userId || m.ReceiverId == userId)
                    .SortByDescending(m => m.SentAt)
                    .Limit(50)
                    .ToListAsync();

                var response = recentMessages.Select(m => new ChatResponseDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.SenderName,
                    MessageText = m.MessageText,
                    MessageType = m.MessageType,
                    DetectedMood = m.DetectedMood,
                    IsRead = m.IsRead,
                    SentAt = m.SentAt
                }).ToList();

                return ApiResponse<List<ChatResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ChatResponseDto>>.ErrorResponse($"Error: {ex.Message}");
            }
        }
    }
}