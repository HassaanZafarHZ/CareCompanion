using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CareOS.Api.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;

        [BsonElement("type")]
        public string Type { get; set; } = string.Empty; // EMERGENCY, MEDICINE, CHECKIN, CHAT, CALL, TASK

        [BsonElement("relatedId")]
        public string? RelatedId { get; set; } // ID of related entity (call, message, etc)

        [BsonElement("isRead")]
        public bool IsRead { get; set; } = false;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("readAt")]
        public DateTime? ReadAt { get; set; }
    }
}