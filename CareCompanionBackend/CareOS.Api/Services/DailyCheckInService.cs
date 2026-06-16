using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Models;
using MongoDB.Driver;

namespace CareOS.Api.Services
{
    public class DailyCheckInService : IDailyCheckInService
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<DailyCheckIn> _checkIns;
        private readonly IMongoCollection<User> _users;
        private readonly IEmergencyService _emergencyService;

        public DailyCheckInService(MongoDbContext context, IEmergencyService emergencyService)
        {
            _context = context;
            _emergencyService = emergencyService;
            _checkIns = _context.GetCollection<DailyCheckIn>("DailyCheckIns");
            _users = _context.GetCollection<User>("Users");
        }

        // CREATE DAILY CHECK-IN
        public async Task<ApiResponse<DailyCheckIn>> CreateCheckInAsync(CreateCheckInDto request)
        {
            try
            {
                var elder = await _users.Find(u => u.Id == request.ElderId).FirstOrDefaultAsync();
                if (elder == null)
                {
                    return ApiResponse<DailyCheckIn>.ErrorResponse("Elder not found");
                }

                var checkIn = new DailyCheckIn
                {
                    ElderId = request.ElderId,
                    ElderName = elder.FullName,
                    FeelingStatus = request.FeelingStatus,
                    Mood = DetermineMood(request.Notes ?? ""),
                    Notes = request.Notes,
                    CheckInTime = DateTime.UtcNow,
                    AlertGenerated = false
                };

                // If elder is NOT GOOD, trigger emergency alert
                if (request.FeelingStatus == "NOT_GOOD")
                {
                    var emergencyDto = new TriggerEmergencyDto
                    {
                        ElderId = request.ElderId,
                        AlertType = "NOT_FEELING_WELL",
                        Message = $"{elder.FullName} reported not feeling well during daily check-in",
                        Location = null
                    };

                    await _emergencyService.TriggerEmergencyAsync(emergencyDto);
                    checkIn.AlertGenerated = true;
                }

                await _checkIns.InsertOneAsync(checkIn);

                return ApiResponse<DailyCheckIn>.SuccessResponse(checkIn, "Daily check-in completed");
            }
            catch (Exception ex)
            {
                return ApiResponse<DailyCheckIn>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET ELDER'S CHECK-INS (Last N days)
        public async Task<ApiResponse<List<DailyCheckIn>>> GetElderCheckInsAsync(string elderId, int days = 7)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);

                var checkIns = await _checkIns
                    .Find(c => c.ElderId == elderId && c.CheckInTime >= startDate)
                    .SortByDescending(c => c.CheckInTime)
                    .ToListAsync();

                return ApiResponse<List<DailyCheckIn>>.SuccessResponse(checkIns, $"Found {checkIns.Count} check-ins");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DailyCheckIn>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET TODAY'S CHECK-IN
        public async Task<ApiResponse<DailyCheckIn>> GetTodayCheckInAsync(string elderId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var checkIn = await _checkIns
                    .Find(c => c.ElderId == elderId && c.CheckInTime >= today && c.CheckInTime < tomorrow)
                    .FirstOrDefaultAsync();

                if (checkIn == null)
                {
                    return ApiResponse<DailyCheckIn>.ErrorResponse("No check-in found for today");
                }

                return ApiResponse<DailyCheckIn>.SuccessResponse(checkIn);
            }
            catch (Exception ex)
            {
                return ApiResponse<DailyCheckIn>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // Simple mood detection (AI mock)
        private string DetermineMood(string notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
                return "Neutral";

            notes = notes.ToLower();

            if (notes.Contains("happy") || notes.Contains("good") || notes.Contains("great") || notes.Contains("excellent"))
                return "Happy";

            if (notes.Contains("sad") || notes.Contains("depressed") || notes.Contains("lonely"))
                return "Sad";

            if (notes.Contains("anxious") || notes.Contains("worried") || notes.Contains("stressed"))
                return "Anxious";

            if (notes.Contains("pain") || notes.Contains("hurt") || notes.Contains("sick"))
                return "Unwell";

            return "Neutral";
        }
    }
}