using CareOS.Api.DTOs;

namespace CareOS.Api.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(string userId);
        Task<ApiResponse<bool>> UpdateLastLoginAsync(string userId);
        Task<ApiResponse<string>> ForgotPasswordAsync(string email);
        Task<ApiResponse<bool>> VerifyResetCodeAsync(string email, string code);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto request);
        Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto request);
        Task<ApiResponse<UserDto>> UpdateProfileAsync(string userId, UpdateProfileDto request);
    }
}