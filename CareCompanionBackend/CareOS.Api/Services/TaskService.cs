
using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Models;
using MongoDB.Driver;


namespace CareOS.Api.Services
{
    public class TaskService : ITaskService
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<CaretakerTask> _tasks;

        public TaskService(MongoDbContext context)
        {
            _context = context;
            _tasks = _context.GetCollection<CaretakerTask>("Tasks");
        }

        // GET ALL TASKS FOR CARETAKER
        public async Task<ApiResponse<List<CaretakerTask>>> GetTasksByCaretakerIdAsync(string caretakerId)
        {
            try
            {
                var tasks = await _tasks.Find(t => t.CaretakerId == caretakerId).SortBy(t => t.DueDate).ToListAsync();
                return ApiResponse<List<CaretakerTask>>.SuccessResponse(tasks);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CaretakerTask>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // CREATE TASK
        public async Task<ApiResponse<CaretakerTask>> CreateTaskAsync(CreateTaskDto request, string caretakerId)
        {
            try
            {
                var task = new CaretakerTask
                {
                    ElderId = request.ElderId,
                    CaretakerId = caretakerId,
                    Title = request.Title,
                    Description = request.Description,
                    Priority = request.Priority,
                    DueDate = request.DueDate,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _tasks.InsertOneAsync(task);

                return ApiResponse<CaretakerTask>.SuccessResponse(task, "Task created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CaretakerTask>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // COMPLETE TASK
        public async Task<ApiResponse<bool>> CompleteTaskAsync(string taskId)
        {
            try
            {
                var update = Builders<CaretakerTask>.Update
                    .Set(t => t.IsCompleted, true)
                    .Set(t => t.CompletedAt, DateTime.UtcNow);

                var result = await _tasks.UpdateOneAsync(t => t.Id == taskId, update);

                if (result.ModifiedCount > 0)
                {
                    return ApiResponse<bool>.SuccessResponse(true, "Task completed");
                }

                return ApiResponse<bool>.ErrorResponse("Task not found");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET ELDER'S TASKS (by date or all)
        public async Task<ApiResponse<List<CaretakerTask>>> GetElderTasksAsync(string elderId, DateTime? date = null)
        {
            try
            {
                FilterDefinition<CaretakerTask> filter;

                if (date.HasValue)
                {
                    var startOfDay = date.Value.Date;
                    var endOfDay = startOfDay.AddDays(1);
                    filter = Builders<CaretakerTask>.Filter.And(
                        Builders<CaretakerTask>.Filter.Eq(t => t.ElderId, elderId),
                        Builders<CaretakerTask>.Filter.Gte(t => t.DueDate, startOfDay),
                        Builders<CaretakerTask>.Filter.Lt(t => t.DueDate, endOfDay)
                    );
                }
                else
                {
                    filter = Builders<CaretakerTask>.Filter.Eq(t => t.ElderId, elderId);
                }

                var tasks = await _tasks.Find(filter).SortBy(t => t.DueDate).ToListAsync();

                return ApiResponse<List<CaretakerTask>>.SuccessResponse(tasks);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CaretakerTask>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET TODAY'S TASKS
        public async Task<ApiResponse<List<CaretakerTask>>> GetTodayTasksAsync(string elderId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var tasks = await _tasks
                    .Find(t => t.ElderId == elderId && t.DueDate >= today && t.DueDate < tomorrow)
                    .SortBy(t => t.DueDate)
                    .ToListAsync();

                return ApiResponse<List<CaretakerTask>>.SuccessResponse(tasks);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CaretakerTask>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // DELETE TASK
        public async Task<ApiResponse<bool>> DeleteTaskAsync(string taskId)
        {
            try
            {
                var result = await _tasks.DeleteOneAsync(t => t.Id == taskId);

                if (result.DeletedCount > 0)
                {
                    return ApiResponse<bool>.SuccessResponse(true, "Task deleted");
                }

                return ApiResponse<bool>.ErrorResponse("Task not found");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }
    }
}