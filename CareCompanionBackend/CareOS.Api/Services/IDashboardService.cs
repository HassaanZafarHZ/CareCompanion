using CareOS.Api.DTOs;

namespace CareOS.Api.Services
{
    public interface IDashboardService
    {
        Task<ApiResponse<ElderDashboardStatsDto>> GetElderDashboardStatsAsync(string elderId);
        Task<ApiResponse<CaretakerDashboardStatsDto>> GetCaretakerDashboardStatsAsync(string caretakerId);
    }
}