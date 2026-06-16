using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CareOS.Api.Models
{
    public class CaretakerNote
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

        [BsonElement("content")]
        public string Content { get; set; } = string.Empty;

        [BsonElement("category")]
        public string Category { get; set; } = "GENERAL"; // GENERAL, MEDICAL, BEHAVIORAL, FAMILY

        [BsonElement("isPrivate")]
        public bool IsPrivate { get; set; } = true; // Only caretaker can see

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}