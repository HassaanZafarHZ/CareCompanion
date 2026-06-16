namespace CareOS.Api.DTOs
{
    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyResetCodeDto
    {
        public string Email { get; set; } = string.Empty;
        public string ResetCode { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string ResetCode { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? EmergencyContact { get; set; }
        public string? ProfilePicture { get; set; }
    }
}