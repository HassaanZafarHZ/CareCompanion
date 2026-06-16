using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CareOS.Api.Models
{
    public class CaretakerTask
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

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("priority")]
        public string Priority { get; set; } = "NORMAL"; // LOW, NORMAL, HIGH, URGENT

        [BsonElement("isCompleted")]
        public bool IsCompleted { get; set; } = false;

        [BsonElement("completedAt")]
        public DateTime? CompletedAt { get; set; }

        [BsonElement("dueDate")]
        public DateTime DueDate { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}