using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Models;
using MongoDB.Driver;

namespace CareOS.Api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<Notification> _notifications;

        public NotificationService(MongoDbContext context)
        {
            _context = context;
            _notifications = _context.GetCollection<Notification>("Notifications");
        }

        // CREATE NOTIFICATION
        public async Task<ApiResponse<Notification>> CreateNotificationAsync(
            string userId,
            string title,
            string message,
            string type,
            string? relatedId = null)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    RelatedId = relatedId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notifications.InsertOneAsync(notification);

                // TODO: Send push notification via FCM/APNS
                // await _pushNotificationService.SendAsync(userId, title, message);

                return ApiResponse<Notification>.SuccessResponse(notification, "Notification created");
            }
            catch (Exception ex)
            {
                return ApiResponse<Notification>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET USER NOTIFICATIONS
        public async Task<ApiResponse<List<Notification>>> GetUserNotificationsAsync(string userId)
        {
            try
            {
                var notifications = await _notifications
                    .Find(n => n.UserId == userId)
                    .SortByDescending(n => n.CreatedAt)
                    .Limit(100)
                    .ToListAsync();

                return ApiResponse<List<Notification>>.SuccessResponse(notifications);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<Notification>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET UNREAD COUNT
        public async Task<ApiResponse<int>> GetUnreadCountAsync(string userId)
        {
            try
            {
                var count = await _notifications.CountDocumentsAsync(n => n.UserId == userId && !n.IsRead);
                return ApiResponse<int>.SuccessResponse((int)count);
            }
            catch (Exception ex)
            {
                return ApiResponse<int>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // MARK AS READ
        public async Task<ApiResponse<bool>> MarkAsReadAsync(string notificationId)
        {
            try
            {
                var update = Builders<Notification>.Update
                    .Set(n => n.IsRead, true)
                    .Set(n => n.ReadAt, DateTime.UtcNow);

                var result = await _notifications.UpdateOneAsync(n => n.Id == notificationId, update);

                return ApiResponse<bool>.SuccessResponse(result.ModifiedCount > 0);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // MARK ALL AS READ
        public async Task<ApiResponse<bool>> MarkAllAsReadAsync(string userId)
        {
            try
            {
                var update = Builders<Notification>.Update
                    .Set(n => n.IsRead, true)
                    .Set(n => n.ReadAt, DateTime.UtcNow);

                var result = await _notifications.UpdateManyAsync(
                    n => n.UserId == userId && !n.IsRead,
                    update
                );

                return ApiResponse<bool>.SuccessResponse(
                    result.ModifiedCount > 0,
                    $"{result.ModifiedCount} notifications marked as read"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // DELETE NOTIFICATION
        public async Task<ApiResponse<bool>> DeleteNotificationAsync(string notificationId)
        {
            try
            {
                var result = await _notifications.DeleteOneAsync(n => n.Id == notificationId);
                return ApiResponse<bool>.SuccessResponse(result.DeletedCount > 0);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }
    }
}