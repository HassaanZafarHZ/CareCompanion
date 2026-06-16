using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CareOS.Api.Models
{
    public class EmergencyAlert
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
        public string CaretakerId { get; set; } = string.Empty;

        [BsonElement("alertType")]
        public string AlertType { get; set; } = "GENERAL";

        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;

        [BsonElement("location")]
        public string? Location { get; set; }

        [BsonElement("isAcknowledged")]
        public bool IsAcknowledged { get; set; } = false;

        [BsonElement("acknowledgedAt")]
        public DateTime? AcknowledgedAt { get; set; }

        [BsonElement("triggeredAt")]
        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("resolvedAt")]
        public DateTime? ResolvedAt { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "PENDING";
    }
}