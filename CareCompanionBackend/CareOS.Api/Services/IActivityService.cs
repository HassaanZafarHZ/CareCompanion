using CareOS.Api.DTOs;
using CareOS.Api.Models;

namespace CareOS.Api.Services
{
    public interface IActivityService
    {
        Task<ApiResponse<ActivitySchedule>> CreateActivityScheduleAsync(CreateActivityScheduleDto request, string caretakerId);
        Task<ApiResponse<bool>> CompleteActivityAsync(CompleteActivityDto request);
        Task<ApiResponse<List<ActivitySchedule>>> GetElderActivitiesAsync(string elderId, DateTime date);
        Task<ApiResponse<List<ActivitySchedule>>> GetTodayActivitiesAsync(string elderId);
    }
}