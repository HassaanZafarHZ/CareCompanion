namespace CareOS.Api.DTOs
{
    public class TriggerEmergencyDto
    {
        public string ElderId { get; set; } = string.Empty;
        public string? EmergencyType { get; set; } = "GENERAL";
        public string? AlertType { get; set; } = "GENERAL";
        public string? Message { get; set; }
        public string? Location { get; set; }
    }

    public class AcknowledgeEmergencyDto
    {
        public string AlertId { get; set; } = string.Empty;
        public string CaretakerId { get; set; } = string.Empty;
    }
}