namespace CareOS.Api.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}