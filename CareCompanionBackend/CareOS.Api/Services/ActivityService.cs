using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Models;
using MongoDB.Driver;

namespace CareOS.Api.Services
{
    public class ActivityService : IActivityService
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<ActivitySchedule> _activities;

        public ActivityService(MongoDbContext context)
        {
            _context = context;
            _activities = _context.GetCollection<ActivitySchedule>("ActivitySchedules");
        }

        public async Task<ApiResponse<ActivitySchedule>> CreateActivityScheduleAsync(CreateActivityScheduleDto request, string caretakerId)
        {
            try
            {
                var schedule = new ActivitySchedule
                {
                    ElderId = request.ElderId,
                    CaretakerId = caretakerId,
                    ActivityType = request.ActivityType,
                    ScheduledTime = request.ScheduledTime,
                    DurationMinutes = request.DurationMinutes,
                    Repeat = request.Repeat,
                    Notes = request.Notes,
                    Date = DateTime.UtcNow.Date,
                    IsCompleted = false
                };

                await _activities.InsertOneAsync(schedule);

                return ApiResponse<ActivitySchedule>.SuccessResponse(schedule, "Activity schedule created");
            }
            catch (Exception ex)
            {
                return ApiResponse<ActivitySchedule>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> CompleteActivityAsync(CompleteActivityDto request)
        {
            try
            {
                var update = Builders<ActivitySchedule>.Update
                    .Set(a => a.IsCompleted, true)
                    .Set(a => a.CompletedAt, request.CompletedAt);

                var result = await _activities.UpdateOneAsync(a => a.Id == request.ScheduleId, update);

                if (result.ModifiedCount > 0)
                {
                    return ApiResponse<bool>.SuccessResponse(true, "Activity marked as completed");
                }

                return ApiResponse<bool>.ErrorResponse("Activity not found");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ActivitySchedule>>> GetElderActivitiesAsync(string elderId, DateTime date)
        {
            try
            {
                var activities = await _activities
                    .Find(a => a.ElderId == elderId && a.Date == date.Date)
                    .ToListAsync();

                return ApiResponse<List<ActivitySchedule>>.SuccessResponse(activities);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ActivitySchedule>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ActivitySchedule>>> GetTodayActivitiesAsync(string elderId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                var activities = await _activities
                    .Find(a => a.ElderId == elderId && a.Date == today)
                    .ToListAsync();

                return ApiResponse<List<ActivitySchedule>>.SuccessResponse(activities);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ActivitySchedule>>.ErrorResponse($"Error: {ex.Message}");
            }
        }
    }
}