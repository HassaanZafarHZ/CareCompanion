namespace CareOS.Api.DTOs
{
    public class UploadAndScanDto
    {
        public string Base64Image { get; set; } = string.Empty;
        public string? ElderName { get; set; }
    }

    public class UploadWithChoiceDto
    {
        public string Base64Image { get; set; } = string.Empty;
        public string? ElderName { get; set; }
        public string ScanMethod { get; set; } = "OCR"; // "OCR" ?? "GEMINI"
    }

    public class ReviewPrescriptionDto
    {
        public string Status { get; set; } = string.Empty; // "APPROVED" ?? "REJECTED"
        public string? Notes { get; set; }
    }

    public class AddMedicineDto
    {
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string? Instructions { get; set; }
    }

    public class EditMedicineDto
    {
        public int MedicineIndex { get; set; } // ??? ?? medicine edit ???? ??
        public string? MedicineName { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Duration { get; set; }
        public string? EditNotes { get; set; }
    }
}
