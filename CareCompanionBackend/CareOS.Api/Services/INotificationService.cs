using CareOS.Api.DTOs;
using CareOS.Api.Models;

namespace CareOS.Api.Services
{
    public interface INotificationService
    {
        Task<ApiResponse<Notification>> CreateNotificationAsync(string userId, string title, string message, string type, string? relatedId = null);
        Task<ApiResponse<List<Notification>>> GetUserNotificationsAsync(string userId);
        Task<ApiResponse<int>> GetUnreadCountAsync(string userId);
        Task<ApiResponse<bool>> MarkAsReadAsync(string notificationId);
        Task<ApiResponse<bool>> MarkAllAsReadAsync(string userId);
        Task<ApiResponse<bool>> DeleteNotificationAsync(string notificationId);
    }
}