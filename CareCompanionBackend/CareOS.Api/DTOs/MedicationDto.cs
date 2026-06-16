namespace CareOS.Api.DTOs
{
    public class CreateMedicationDto
    {
        public string ElderId { get; set; } = string.Empty;
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public List<string> ScheduleTimes { get; set; } = new(); // ["08:00 AM", "08:00 PM"]
        public string? PrescriptionImage { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ApproveMedicationDto
    {
        public string MedicationId { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public string? Notes { get; set; }
    }

    public class ConfirmMedicationDto
    {
        public string MedicationId { get; set; } = string.Empty;
        public string ScheduleTime { get; set; } = string.Empty; // "08:00 AM"
        public DateTime TakenAt { get; set; } = DateTime.UtcNow;
    }
}
