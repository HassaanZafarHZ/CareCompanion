using CareOS.Api.DTOs;
using CareOS.Api.Models;

namespace CareOS.Api.Services
{
    public interface IDietService
    {
        Task<ApiResponse<DietSchedule>> CreateDietScheduleAsync(CreateDietScheduleDto request, string caretakerId);
        Task<ApiResponse<bool>> CompleteDietAsync(CompleteDietDto request);
        Task<ApiResponse<List<DietSchedule>>> GetElderDietScheduleAsync(string elderId, DateTime date);
        Task<ApiResponse<List<DietSchedule>>> GetTodayDietAsync(string elderId);
    }
}