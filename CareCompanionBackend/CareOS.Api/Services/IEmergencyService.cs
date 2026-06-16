using CareOS.Api.DTOs;
using CareOS.Api.Models;

namespace CareOS.Api.Services
{
    public interface IEmergencyService
    {
        Task<ApiResponse<EmergencyAlert>> TriggerEmergencyAsync(TriggerEmergencyDto request);
        Task<ApiResponse<EmergencyAlert>> AcknowledgeEmergencyAsync(AcknowledgeEmergencyDto request);
        Task<ApiResponse<List<EmergencyAlert>>> GetElderAlertsAsync(string elderId);
        Task<ApiResponse<List<EmergencyAlert>>> GetCaretakerAlertsAsync(string caretakerId);
        Task<ApiResponse<List<EmergencyAlert>>> GetPendingAlertsAsync(string caretakerId);
        Task<ApiResponse<bool>> ResolveAlertAsync(string alertId);
    }
}