using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CareOS.Api.Models
{
    public class AuditLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("userName")]
        public string UserName { get; set; } = string.Empty;

        [BsonElement("userRole")]
        public string UserRole { get; set; } = string.Empty;

        [BsonElement("action")]
        public string Action { get; set; } = string.Empty; // LOGIN, LOGOUT, EMERGENCY_TRIGGERED, MEDICINE_TAKEN, etc.

        [BsonElement("entityType")]
        public string EntityType { get; set; } = string.Empty; // USER, MEDICATION, EMERGENCY, ASSIGNMENT

        [BsonElement("entityId")]
        public string? EntityId { get; set; }

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("ipAddress")]
        public string? IpAddress { get; set; }

        [BsonElement("userAgent")]
        public string? UserAgent { get; set; }

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [BsonElement("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}