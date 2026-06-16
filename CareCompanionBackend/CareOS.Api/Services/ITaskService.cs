using CareOS.Api.DTOs;
using CareOS.Api.Models;

namespace CareOS.Api.Services
{
    public interface ITaskService
    {
        Task<ApiResponse<CaretakerTask>> CreateTaskAsync(CreateTaskDto request, string caretakerId);
        Task<ApiResponse<bool>> CompleteTaskAsync(string taskId);
        Task<ApiResponse<List<CaretakerTask>>> GetElderTasksAsync(string elderId, DateTime? date = null);
        Task<ApiResponse<List<CaretakerTask>>> GetTodayTasksAsync(string elderId);
        Task<ApiResponse<List<CaretakerTask>>> GetTasksByCaretakerIdAsync(string caretakerId);
        Task<ApiResponse<bool>> DeleteTaskAsync(string taskId);
    }
}