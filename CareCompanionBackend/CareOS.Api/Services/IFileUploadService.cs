using CareOS.Api.DTOs;

namespace CareOS.Api.Services
{
    public interface IFileUploadService
    {
        Task<ApiResponse<string>> UploadImageAsync(IFormFile file, string folder);
        Task<ApiResponse<bool>> DeleteImageAsync(string filePath);
        Task<ApiResponse<string>> UploadBase64ImageAsync(string base64String, string folder, string fileName);
    }
}