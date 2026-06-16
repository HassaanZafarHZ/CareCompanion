namespace CareOS.Api.DTOs
{
    public class PrescriptionAnalysisDto
    {
        public List<ExtractedMedicine> Medicines { get; set; } = new();
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime? PrescriptionDate { get; set; }
        public string RawText { get; set; } = string.Empty;
    }

    public class ExtractedMedicine
    {
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public List<string> SuggestedTimes { get; set; } = new(); // ["08:00 AM", "08:00 PM"]
        public List<string> Warnings { get; set; } = new();
    }

    public class UploadPrescriptionDto
    {
        public string ElderId { get; set; } = string.Empty;
        public string Base64Image { get; set; } = string.Empty; // Image as base64 string
        public string Notes { get; set; } = string.Empty;
    }

    public class PrescriptionDto
    {
        public string Id { get; set; } = string.Empty;
        public string ElderId { get; set; } = string.Empty;
        public string ElderName { get; set; } = string.Empty;
        public string CaretakerId { get; set; } = string.Empty;
        public string PrescriptionImageUrl { get; set; } = string.Empty;
        public PrescriptionAnalysisDto Analysis { get; set; } = new();
        public bool IsApproved { get; set; }
        public DateTime UploadedAt { get; set; }
        public string Status { get; set; } = string.Empty; // PENDING, APPROVED, REJECTED
    }
}