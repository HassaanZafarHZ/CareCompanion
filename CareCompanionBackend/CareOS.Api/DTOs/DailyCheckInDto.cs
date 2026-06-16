namespace CareOS.Api.DTOs
{
    public class CreateCheckInDto
    {
        public string ElderId { get; set; } = string.Empty;
        public string FeelingStatus { get; set; } = string.Empty; // "GOOD", "NOT_GOOD"
        public string? Notes { get; set; }
    }
}