using CareOS.Api.DTOs;
using CareOS.Api.Models;

namespace CareOS.Api.Services
{
    public interface IAuditLogService
    {
        Task<ApiResponse<AuditLog>> LogActionAsync(
            string userId,
            string userName,
            string userRole,
            string action,
            string entityType,
            string? entityId = null,
            string? description = null,
            string? ipAddress = null);

        Task<ApiResponse<List<AuditLog>>> GetUserLogsAsync(string userId, int days = 30);
        Task<ApiResponse<List<AuditLog>>> GetAllLogsAsync(int days = 7);
        Task<ApiResponse<List<AuditLog>>> GetLogsByActionAsync(string action, int days = 7);
        Task<ApiResponse<List<AuditLog>>> GetLogsByEntityAsync(string entityType, string entityId);
    }
}