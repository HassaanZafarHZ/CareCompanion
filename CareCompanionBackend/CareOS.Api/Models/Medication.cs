using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CareOS.Api.Models
{
    public class Medication
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("elderId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ElderId { get; set; } = string.Empty;

        [BsonElement("caretakerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CaretakerId { get; set; } = string.Empty;

        [BsonElement("medicineName")]
        public string MedicineName { get; set; } = string.Empty;

        [BsonElement("dosage")]
        public string Dosage { get; set; } = string.Empty; // "500mg", "2 tablets"

        [BsonElement("frequency")]
        public string Frequency { get; set; } = string.Empty; // "Twice daily", "After meals"

        [BsonElement("schedules")]
        public List<MedicationSchedule> Schedules { get; set; } = new();

        [BsonElement("prescriptionImage")]
        public string? PrescriptionImage { get; set; } // OCR se scan kiya hua

        [BsonElement("aiSuggested")]
        public bool AiSuggested { get; set; } = false; // AI ne suggest kiya?

        [BsonElement("approvedByCaretaker")]
        public bool ApprovedByCaretaker { get; set; } = false;

        [BsonElement("warnings")]
        public List<string> Warnings { get; set; } = new(); // Medicine interactions

        [BsonElement("startDate")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [BsonElement("endDate")]
        public DateTime? EndDate { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Medicine ka time schedule
    public class MedicationSchedule
    {
        public string Time { get; set; } = string.Empty; // "08:00 AM", "02:00 PM"
        public bool IsTaken { get; set; } = false;
        public DateTime? TakenAt { get; set; }
        public string Day { get; set; } = string.Empty; // "Monday", "Everyday"
    }
}