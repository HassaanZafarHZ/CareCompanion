using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Models;
using MongoDB.Driver;

namespace CareOS.Api.Services
{
    public class DietService : IDietService
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<DietSchedule> _dietSchedules;

        public DietService(MongoDbContext context)
        {
            _context = context;
            _dietSchedules = _context.GetCollection<DietSchedule>("DietSchedules");
        }

        // CREATE DIET SCHEDULE
        public async Task<ApiResponse<DietSchedule>> CreateDietScheduleAsync(CreateDietScheduleDto request, string caretakerId)
        {
            try
            {
                var schedule = new DietSchedule
                {
                    ElderId = request.ElderId,
                    CaretakerId = caretakerId,
                    MealType = request.MealType,
                    FoodItems = request.FoodItems,
                    ScheduledTime = request.ScheduledTime,
                    Calories = request.Calories,
                    Notes = request.Notes,
                    Date = DateTime.UtcNow.Date,
                    IsCompleted = false
                };

                await _dietSchedules.InsertOneAsync(schedule);

                return ApiResponse<DietSchedule>.SuccessResponse(schedule, "Diet schedule created");
            }
            catch (Exception ex)
            {
                return ApiResponse<DietSchedule>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // COMPLETE DIET (Elder confirms meal eaten)
        public async Task<ApiResponse<bool>> CompleteDietAsync(CompleteDietDto request)
        {
            try
            {
                var update = Builders<DietSchedule>.Update
                    .Set(d => d.IsCompleted, true)
                    .Set(d => d.CompletedAt, request.CompletedAt);

                var result = await _dietSchedules.UpdateOneAsync(d => d.Id == request.ScheduleId, update);

                if (result.ModifiedCount > 0)
                {
                    return ApiResponse<bool>.SuccessResponse(true, "Meal marked as completed");
                }

                return ApiResponse<bool>.ErrorResponse("Schedule not found");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET DIET SCHEDULE FOR SPECIFIC DATE
        public async Task<ApiResponse<List<DietSchedule>>> GetElderDietScheduleAsync(string elderId, DateTime date)
        {
            try
            {
                var schedules = await _dietSchedules
                    .Find(d => d.ElderId == elderId && d.Date == date.Date)
                    .ToListAsync();

                return ApiResponse<List<DietSchedule>>.SuccessResponse(schedules);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DietSchedule>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET TODAY'S DIET
        public async Task<ApiResponse<List<DietSchedule>>> GetTodayDietAsync(string elderId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                var schedules = await _dietSchedules
                    .Find(d => d.ElderId == elderId && d.Date == today)
                    .ToListAsync();

                return ApiResponse<List<DietSchedule>>.SuccessResponse(schedules);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DietSchedule>>.ErrorResponse($"Error: {ex.Message}");
            }
        }
    }
}