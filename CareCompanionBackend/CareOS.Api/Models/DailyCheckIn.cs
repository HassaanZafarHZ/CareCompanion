using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CareOS.Api.Models
{
    public class DailyCheckIn
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("elderId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ElderId { get; set; } = string.Empty;

        [BsonElement("elderName")]
        public string ElderName { get; set; } = string.Empty;

        [BsonElement("feelingStatus")]
        public string FeelingStatus { get; set; } = string.Empty; // "GOOD", "NOT_GOOD"

        [BsonElement("mood")]
        public string Mood { get; set; } = string.Empty; // "Happy", "Sad", "Anxious"

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("checkInTime")]
        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

        [BsonElement("alertGenerated")]
        public bool AlertGenerated { get; set; } = false; // Agar "NOT_GOOD" toh alert
    }
}