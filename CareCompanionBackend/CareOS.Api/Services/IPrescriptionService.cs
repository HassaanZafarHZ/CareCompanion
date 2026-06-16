using CareOS.Api.DTOs;
using CareOS.Api.Models;

namespace CareOS.Api.Services
{
    public interface IPrescriptionService
    {
        Task<ApiResponse<Prescription>> UploadPrescriptionAsync(UploadPrescriptionDto request);
        Task<ApiResponse<Prescription>> ApprovePrescriptionAsync(string prescriptionId, bool isApproved);
        Task<ApiResponse<List<Prescription>>> GetPendingPrescriptionsAsync(string caretakerId);
        Task<ApiResponse<List<Prescription>>> GetElderPrescriptionsAsync(string elderId);
        Task<ApiResponse<Prescription>> GetPrescriptionByIdAsync(string prescriptionId);
        Task<ApiResponse<Prescription>> CreatePrescriptionAsync(Prescription prescription);
        Task<ApiResponse<Prescription>> UpdatePrescriptionStatusAsync(string prescriptionId, string status, string? notes);
        Task<ApiResponse<Prescription>> AddMedicineAsync(string prescriptionId, string caretakerId, ExtractedMedicine medicine, string? editNotes);
        Task<ApiResponse<Prescription>> EditMedicineAsync(string prescriptionId, string caretakerId, int medicineIndex, EditMedicineDto editData);
        Task<string?> GetAssignedCaretakerIdAsync(string elderId);
        Task<ApiResponse<bool>> DeletePrescriptionAsync(string prescriptionId, string elderId);
    }
}