using CareOS.Api.DTOs;
using CareOS.Api.Helpers;
using CareOS.Api.Models;

namespace CareOS.Api.Services
{
    public interface IChatService
    {
        Task<ApiResponse<ChatMessage>> SendMessageAsync(SendMessageDto request);
        Task<ApiResponse<PaginatedResult<ChatResponseDto>>> GetConversationAsync(string userId, string otherUserId, int page = 1, int pageSize = 50);
        Task<ApiResponse<int>> GetUnreadCountAsync(string userId);
        Task<ApiResponse<bool>> MarkAsReadAsync(string messageId);
        Task<ApiResponse<List<ChatResponseDto>>> GetRecentChatsAsync(string userId);
    }
}