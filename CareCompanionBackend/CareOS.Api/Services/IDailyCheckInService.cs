using CareOS.Api.DTOs;
using CareOS.Api.Models;

namespace CareOS.Api.Services
{
    public interface IDailyCheckInService
    {
        Task<ApiResponse<DailyCheckIn>> CreateCheckInAsync(CreateCheckInDto request);
        Task<ApiResponse<List<DailyCheckIn>>> GetElderCheckInsAsync(string elderId, int days = 7);
        Task<ApiResponse<DailyCheckIn>> GetTodayCheckInAsync(string elderId);
    }
}