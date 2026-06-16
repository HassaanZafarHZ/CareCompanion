using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Models;
using MongoDB.Driver;

namespace CareOS.Api.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<AuditLog> _auditLogs;

        public AuditLogService(MongoDbContext context)
        {
            _context = context;
            _auditLogs = _context.GetCollection<AuditLog>("AuditLogs");
        }

        // LOG ACTION
        public async Task<ApiResponse<AuditLog>> LogActionAsync(
            string userId,
            string userName,
            string userRole,
            string action,
            string entityType,
            string? entityId = null,
            string? description = null,
            string? ipAddress = null)
        {
            try
            {
                var log = new AuditLog
                {
                    UserId = userId,
                    UserName = userName,
                    UserRole = userRole,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Description = description ?? $"{userName} performed {action}",
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow
                };

                await _auditLogs.InsertOneAsync(log);

                return ApiResponse<AuditLog>.SuccessResponse(log, "Action logged");
            }
            catch (Exception ex)
            {
                return ApiResponse<AuditLog>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET USER'S LOGS
        public async Task<ApiResponse<List<AuditLog>>> GetUserLogsAsync(string userId, int days = 30)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);

                var logs = await _auditLogs
                    .Find(l => l.UserId == userId && l.Timestamp >= startDate)
                    .SortByDescending(l => l.Timestamp)
                    .ToListAsync();

                return ApiResponse<List<AuditLog>>.SuccessResponse(logs);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<AuditLog>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET ALL LOGS (Admin/Caretaker view)
        public async Task<ApiResponse<List<AuditLog>>> GetAllLogsAsync(int days = 7)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);

                var logs = await _auditLogs
                    .Find(l => l.Timestamp >= startDate)
                    .SortByDescending(l => l.Timestamp)
                    .Limit(500)
                    .ToListAsync();

                return ApiResponse<List<AuditLog>>.SuccessResponse(logs);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<AuditLog>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET LOGS BY ACTION TYPE
        public async Task<ApiResponse<List<AuditLog>>> GetLogsByActionAsync(string action, int days = 7)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);

                var logs = await _auditLogs
                    .Find(l => l.Action == action && l.Timestamp >= startDate)
                    .SortByDescending(l => l.Timestamp)
                    .ToListAsync();

                return ApiResponse<List<AuditLog>>.SuccessResponse(logs);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<AuditLog>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET LOGS BY ENTITY
        public async Task<ApiResponse<List<AuditLog>>> GetLogsByEntityAsync(string entityType, string entityId)
        {
            try
            {
                var logs = await _auditLogs
                    .Find(l => l.EntityType == entityType && l.EntityId == entityId)
                    .SortByDescending(l => l.Timestamp)
                    .ToListAsync();

                return ApiResponse<List<AuditLog>>.SuccessResponse(logs);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<AuditLog>>.ErrorResponse($"Error: {ex.Message}");
            }
        }
    }
}