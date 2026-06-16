namespace CareOS.Api.DTOs
{
    public class CreateDietScheduleDto
    {
        public string ElderId { get; set; } = string.Empty;
        public string MealType { get; set; } = string.Empty; // "BREAKFAST", "LUNCH", "DINNER"
        public List<string> FoodItems { get; set; } = new();
        public string ScheduledTime { get; set; } = string.Empty; // "08:00 AM"
        public int? Calories { get; set; }
        public string? Notes { get; set; }
    }

    public class CompleteDietDto
    {
        public string ScheduleId { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}