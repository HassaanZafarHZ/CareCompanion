using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Models;
using MongoDB.Driver;

namespace CareOS.Api.Services
{
    public class EmergencyService : IEmergencyService
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<EmergencyAlert> _alerts;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<ElderCaretakerAssignment> _assignments;

        public EmergencyService(MongoDbContext context)
        {
            _context = context;
            _alerts = _context.GetCollection<EmergencyAlert>("EmergencyAlerts");
            _users = _context.GetCollection<User>("Users");
            _assignments = _context.GetCollection<ElderCaretakerAssignment>("Assignments");
        }

        // TRIGGER EMERGENCY ALERT
        public async Task<ApiResponse<EmergencyAlert>> TriggerEmergencyAsync(TriggerEmergencyDto request)
        {
            try
            {
                // Get elder details
                var elder = await _users.Find(u => u.Id == request.ElderId).FirstOrDefaultAsync();
                if (elder == null)
                {
                    return ApiResponse<EmergencyAlert>.ErrorResponse("Elder not found");
                }

                // Get APPROVED assignment only
                var assignment = await _assignments
                    .Find(a => a.ElderId == request.ElderId && a.IsActive && a.Status == "APPROVED")
                    .FirstOrDefaultAsync();

                if (assignment == null)
                {
                    return ApiResponse<EmergencyAlert>.ErrorResponse("No caretaker assigned. Please assign a caretaker first.");
                }

                // Create emergency alert
                var alert = new EmergencyAlert
                {
                    ElderId = request.ElderId,
                    ElderName = elder.FullName,
                    CaretakerId = assignment.CaretakerId,
                    AlertType = request.EmergencyType ?? "GENERAL",
                    Message = request.Message ?? "Emergency! Need help immediately!",
                    Location = request.Location,
                    IsAcknowledged = false,
                    Status = "PENDING",
                    TriggeredAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _alerts.InsertOneAsync(alert);

                return ApiResponse<EmergencyAlert>.SuccessResponse(alert, "🚨 Emergency alert sent to your caretaker!");
            }
            catch (Exception ex)
            {
                return ApiResponse<EmergencyAlert>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // ACKNOWLEDGE EMERGENCY
        public async Task<ApiResponse<EmergencyAlert>> AcknowledgeEmergencyAsync(AcknowledgeEmergencyDto request)
        {
            try
            {
                var alert = await _alerts.Find(a => a.Id == request.AlertId).FirstOrDefaultAsync();
                if (alert == null)
                {
                    return ApiResponse<EmergencyAlert>.ErrorResponse("Alert not found");
                }

                if (alert.CaretakerId != request.CaretakerId)
                {
                    return ApiResponse<EmergencyAlert>.ErrorResponse("Unauthorized");
                }

                var update = Builders<EmergencyAlert>.Update
                    .Set(a => a.IsAcknowledged, true)
                    .Set(a => a.AcknowledgedAt, DateTime.UtcNow)
                    .Set(a => a.Status, "ACKNOWLEDGED");

                await _alerts.UpdateOneAsync(a => a.Id == request.AlertId, update);
                alert.IsAcknowledged = true;
                alert.Status = "ACKNOWLEDGED";

                return ApiResponse<EmergencyAlert>.SuccessResponse(alert, "Emergency acknowledged");
            }
            catch (Exception ex)
            {
                return ApiResponse<EmergencyAlert>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET ELDER'S ALERTS
        public async Task<ApiResponse<List<EmergencyAlert>>> GetElderAlertsAsync(string elderId)
        {
            try
            {
                var alerts = await _alerts.Find(a => a.ElderId == elderId).SortByDescending(a => a.CreatedAt).ToListAsync();
                return ApiResponse<List<EmergencyAlert>>.SuccessResponse(alerts);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<EmergencyAlert>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET CARETAKER'S ALERTS
        public async Task<ApiResponse<List<EmergencyAlert>>> GetCaretakerAlertsAsync(string caretakerId)
        {
            try
            {
                var alerts = await _alerts.Find(a => a.CaretakerId == caretakerId).SortByDescending(a => a.CreatedAt).ToListAsync();
                return ApiResponse<List<EmergencyAlert>>.SuccessResponse(alerts);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<EmergencyAlert>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET PENDING ALERTS
        public async Task<ApiResponse<List<EmergencyAlert>>> GetPendingAlertsAsync(string caretakerId)
        {
            try
            {
                var alerts = await _alerts
                    .Find(a => a.CaretakerId == caretakerId && a.Status == "PENDING" && !a.IsAcknowledged)
                    .SortByDescending(a => a.CreatedAt)
                    .ToListAsync();
                return ApiResponse<List<EmergencyAlert>>.SuccessResponse(alerts);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<EmergencyAlert>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // RESOLVE ALERT
        public async Task<ApiResponse<bool>> ResolveAlertAsync(string alertId)
        {
            try
            {
                var update = Builders<EmergencyAlert>.Update
                    .Set(a => a.Status, "RESOLVED")
                    .Set(a => a.IsAcknowledged, true)
                    .Set(a => a.ResolvedAt, DateTime.UtcNow);

                var result = await _alerts.UpdateOneAsync(a => a.Id == alertId, update);
                return result.ModifiedCount > 0
                    ? ApiResponse<bool>.SuccessResponse(true, "Alert resolved")
                    : ApiResponse<bool>.ErrorResponse("Alert not found");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }
    }
}