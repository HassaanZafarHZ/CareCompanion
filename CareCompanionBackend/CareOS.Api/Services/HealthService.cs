using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Models;
using MongoDB.Driver;

namespace CareOS.Api.Services
{
    public class HealthService : IHealthService
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<User> _users;
        private readonly IEmergencyService _emergencyService;

        public HealthService(MongoDbContext context, IEmergencyService emergencyService)
        {
            _context = context;
            _emergencyService = emergencyService;
            _users = _context.GetCollection<User>("Users");
        }

        // RECORD BLOOD PRESSURE
        public async Task<ApiResponse<User>> RecordBloodPressureAsync(string userId, int systolic, int diastolic)
        {
            try
            {
                var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                if (user == null)
                {
                    return ApiResponse<User>.ErrorResponse("User not found");
                }

                // Determine BP status
                string status = "Normal";
                bool shouldAlert = false;

                if (systolic >= 140 || diastolic >= 90)
                {
                    status = "High";
                    shouldAlert = true;
                }
                else if (systolic < 90 || diastolic < 60)
                {
                    status = "Low";
                    shouldAlert = true;
                }

                var bpReading = new BloodPressureReading
                {
                    Systolic = systolic,
                    Diastolic = diastolic,
                    Status = status,
                    RecordedAt = DateTime.UtcNow
                };

                // Update user's current BP
                var update = Builders<User>.Update.Set(u => u.CurrentBP, bpReading);
                await _users.UpdateOneAsync(u => u.Id == userId, update);

                // If BP is abnormal, trigger emergency alert
                if (shouldAlert && user.Role == "ELDER")
                {
                    var emergencyDto = new TriggerEmergencyDto
                    {
                        ElderId = userId,
                        AlertType = "ABNORMAL_BP",
                        Message = $"Blood Pressure Alert: {systolic}/{diastolic} ({status})",
                        Location = null
                    };

                    await _emergencyService.TriggerEmergencyAsync(emergencyDto);
                }

                user.CurrentBP = bpReading;

                return ApiResponse<User>.SuccessResponse(user, $"BP recorded: {systolic}/{diastolic} - {status}");
            }
            catch (Exception ex)
            {
                return ApiResponse<User>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET BP HISTORY (Mock - in real app store in separate collection)
        public async Task<ApiResponse<List<BloodPressureReading>>> GetBPHistoryAsync(string userId, int days = 30)
        {
            try
            {
                var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                if (user == null)
                {
                    return ApiResponse<List<BloodPressureReading>>.ErrorResponse("User not found");
                }

                // Mock history (in production, store in separate BPReadings collection)
                var history = new List<BloodPressureReading>();
                if (user.CurrentBP != null)
                {
                    history.Add(user.CurrentBP);
                }

                return ApiResponse<List<BloodPressureReading>>.SuccessResponse(history);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BloodPressureReading>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET LATEST BP
        public async Task<ApiResponse<BloodPressureReading>> GetLatestBPAsync(string userId)
        {
            try
            {
                var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                if (user == null)
                {
                    return ApiResponse<BloodPressureReading>.ErrorResponse("User not found");
                }

                if (user.CurrentBP == null)
                {
                    return ApiResponse<BloodPressureReading>.ErrorResponse("No BP reading found");
                }

                return ApiResponse<BloodPressureReading>.SuccessResponse(user.CurrentBP);
            }
            catch (Exception ex)
            {
                return ApiResponse<BloodPressureReading>.ErrorResponse($"Error: {ex.Message}");
            }
        }
    }
}