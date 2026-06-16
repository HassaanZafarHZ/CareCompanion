using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CareOS.Api.Models
{
    public class ElderCaretakerAssignment
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

        [BsonElement("elderName")]
        public string ElderName { get; set; } = string.Empty;

        [BsonElement("caretakerName")]
        public string CaretakerName { get; set; } = string.Empty;

        [BsonElement("assignedAt")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("approvedAt")]
        public DateTime? ApprovedAt { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("status")]
        public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED

        [BsonElement("notes")]
        public string? Notes { get; set; } // Caretaker ke private notes

        [BsonElement("elderPhone")]
        public string? ElderPhone { get; set; }

        [BsonElement("elderEmail")]
        public string? ElderEmail { get; set; }
    }
}