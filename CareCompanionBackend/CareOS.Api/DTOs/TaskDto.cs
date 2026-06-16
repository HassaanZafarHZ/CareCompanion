namespace CareOS.Api.DTOs
{
    public class CreateTaskDto
{
    public string ElderId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "NORMAL"; // LOW, NORMAL, HIGH, URGENT
    public DateTime DueDate { get; set; }
}
}