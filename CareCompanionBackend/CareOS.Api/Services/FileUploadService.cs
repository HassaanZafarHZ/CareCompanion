using CareOS.Api.DTOs;

namespace CareOS.Api.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadsFolder;

        public FileUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads");

            // Create uploads folder if not exists
            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }
        }

        // UPLOAD IMAGE FILE
        public async Task<ApiResponse<string>> UploadImageAsync(IFormFile file, string folder)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return ApiResponse<string>.ErrorResponse("No file provided");
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return ApiResponse<string>.ErrorResponse("Invalid file type. Only images allowed.");
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return ApiResponse<string>.ErrorResponse("File size exceeds 5MB limit");
                }

                // Create folder path
                var folderPath = Path.Combine(_uploadsFolder, folder);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(folderPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative path
                var relativePath = $"/Uploads/{folder}/{fileName}";

                return ApiResponse<string>.SuccessResponse(relativePath, "File uploaded successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Upload failed: {ex.Message}");
            }
        }

        // UPLOAD BASE64 IMAGE
        public async Task<ApiResponse<string>> UploadBase64ImageAsync(string base64String, string folder, string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(base64String))
                {
                    return ApiResponse<string>.ErrorResponse("No image data provided");
                }

                // Remove data:image/jpeg;base64, prefix if exists
                if (base64String.Contains(","))
                {
                    base64String = base64String.Split(',')[1];
                }

                byte[] imageBytes = Convert.FromBase64String(base64String);

                // Validate size (max 5MB)
                if (imageBytes.Length > 5 * 1024 * 1024)
                {
                    return ApiResponse<string>.ErrorResponse("Image size exceeds 5MB limit");
                }

                // Create folder path
                var folderPath = Path.Combine(_uploadsFolder, folder);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Generate filename
                var uniqueFileName = $"{fileName}_{Guid.NewGuid()}.jpg";
                var filePath = Path.Combine(folderPath, uniqueFileName);

                // Save file
                await File.WriteAllBytesAsync(filePath, imageBytes);

                // Return relative path
                var relativePath = $"/Uploads/{folder}/{uniqueFileName}";

                return ApiResponse<string>.SuccessResponse(relativePath, "Image uploaded successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Upload failed: {ex.Message}");
            }
        }

        // DELETE IMAGE
        public async Task<ApiResponse<bool>> DeleteImageAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.ContentRootPath, filePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    await Task.CompletedTask;
                    return ApiResponse<bool>.SuccessResponse(true, "File deleted successfully");
                }

                return ApiResponse<bool>.ErrorResponse("File not found");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Delete failed: {ex.Message}");
            }
        }
    }
}