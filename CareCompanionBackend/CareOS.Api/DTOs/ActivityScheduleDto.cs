namespace CareOS.Api.DTOs
{
    public class CreateActivityScheduleDto
    {
        public string ElderId { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty; // "SLEEP", "WALK"
        public string ScheduledTime { get; set; } = string.Empty; // "10:00 PM"
        public int DurationMinutes { get; set; }
        public string Repeat { get; set; } = "DAILY"; // "DAILY", "WEEKLY"
        public string? Notes { get; set; }
    }

    public class CompleteActivityDto
    {
        public string ScheduleId { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}