using CareOS.Api.DTOs;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CareOS.Api.Models
{
    public class Prescription
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("elderId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ElderId { get; set; } = string.Empty;

        [BsonElement("elderName")]
        public string ElderName { get; set; } = string.Empty;

        [BsonElement("caretakerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfNull]
        public string? CaretakerId { get; set; }  // Nullable banaya

        [BsonElement("prescriptionImageUrl")]
        public string PrescriptionImageUrl { get; set; } = string.Empty;

        [BsonElement("base64Image")]
        public string Base64Image { get; set; } = string.Empty;

        [BsonElement("analysis")]
        public PrescriptionAnalysisDto Analysis { get; set; } = new();

        [BsonElement("isApproved")]
        public bool IsApproved { get; set; } = false;

        [BsonElement("status")]
        public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("uploadedAt")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("approvedAt")]
        public DateTime? ApprovedAt { get; set; }

        [BsonElement("editedBy")]
        [BsonIgnoreIfNull]
        public string? EditedBy { get; set; } // Caretaker کا ID

        [BsonElement("medicineAdditions")]
        [BsonIgnoreIfNull]
        public List<ExtractedMedicine>? AddedMedicines { get; set; } = new();

        [BsonElement("editNotes")]
        public string? EditNotes { get; set; }
    }
}