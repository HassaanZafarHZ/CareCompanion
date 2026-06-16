using System.ComponentModel.DataAnnotations;

namespace CareOS.Api.DTOs
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = string.Empty;

        [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN must be exactly 4 digits")]
        public string? Pin { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(ELDER|CARETAKER)$", ErrorMessage = "Role must be either ELDER or CARETAKER")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }
        public string? EmergencyContact { get; set; }
    }
}