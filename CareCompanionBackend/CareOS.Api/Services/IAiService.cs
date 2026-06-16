using CareOS.Api.DTOs;

namespace CareOS.Api.Services
{
    public interface IAiService
    {
        Task<ApiResponse<PrescriptionAnalysisDto>> AnalyzePrescriptionAsync(string base64Image);
        Task<ApiResponse<string>> DetectMoodFromTextAsync(string messageText);
        Task<ApiResponse<List<string>>> CheckMedicineInteractionsAsync(List<string> medicineNames);
    }
}