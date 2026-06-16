namespace CareOS.Api.DTOs
{
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Pin { get; set; } // Elder ke liye PIN login
        public string Role { get; set; } = string.Empty; // "ELDER" ya "CARETAKER"
    }
}