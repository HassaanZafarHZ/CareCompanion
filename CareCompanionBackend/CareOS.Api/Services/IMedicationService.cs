using CareOS.Api.DTOs;
using CareOS.Api.Models;

namespace CareOS.Api.Services
{
    public interface IMedicationService
    {
        Task<ApiResponse<Medication>> CreateMedicationAsync(CreateMedicationDto request, string caretakerId);
        Task<ApiResponse<Medication>> ApproveMedicationAsync(ApproveMedicationDto request);
        Task<ApiResponse<bool>> ConfirmMedicationTakenAsync(ConfirmMedicationDto request);
        Task<ApiResponse<List<Medication>>> GetElderMedicationsAsync(string elderId);
        Task<ApiResponse<List<Medication>>> GetPendingApprovalsAsync(string caretakerId);
        Task<ApiResponse<Medication>> GetMedicationByIdAsync(string medicationId);
        Task<ApiResponse<bool>> DeleteMedicationAsync(string medicationId);
    }
}