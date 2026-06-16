using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CareOS.Api.Models
{
    public class ChatMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("senderId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string SenderId { get; set; } = string.Empty;

        [BsonElement("receiverId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ReceiverId { get; set; } = string.Empty;

        [BsonElement("senderName")]
        public string SenderName { get; set; } = string.Empty;

        [BsonElement("messageText")]
        public string MessageText { get; set; } = string.Empty;

        [BsonElement("messageType")]
        public string MessageType { get; set; } = "TEXT"; // "TEXT", "VOICE", "IMAGE"

        [BsonElement("voiceUrl")]
        public string? VoiceUrl { get; set; }

        [BsonElement("detectedMood")]
        public string? DetectedMood { get; set; } // AI se detect kiya mood

        [BsonElement("isRead")]
        public bool IsRead { get; set; } = false;

        [BsonElement("sentAt")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}