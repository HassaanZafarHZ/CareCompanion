using CareOS.Api.DTOs;
using CareOS.Api.Models;

namespace CareOS.Api.Services
{
    public interface IHealthService
    {
        Task<ApiResponse<User>> RecordBloodPressureAsync(string userId, int systolic, int diastolic);
        Task<ApiResponse<List<BloodPressureReading>>> GetBPHistoryAsync(string userId, int days = 30);
        Task<ApiResponse<BloodPressureReading>> GetLatestBPAsync(string userId);
    }

    public class RecordBPDto
    {
        public int Systolic { get; set; }
        public int Diastolic { get; set; }
    }
}