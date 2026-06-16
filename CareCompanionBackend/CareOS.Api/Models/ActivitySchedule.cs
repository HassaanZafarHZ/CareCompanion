using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CareOS.Api.Models
{
    public class ActivitySchedule
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

        [BsonElement("activityType")]
        public string ActivityType { get; set; } = string.Empty; // "SLEEP", "WALK", "EXERCISE"

        [BsonElement("scheduledTime")]
        public string ScheduledTime { get; set; } = string.Empty; // "10:00 PM"

        [BsonElement("duration")]
        public int DurationMinutes { get; set; } // 30 minutes walk

        [BsonElement("isCompleted")]
        public bool IsCompleted { get; set; } = false;

        [BsonElement("completedAt")]
        public DateTime? CompletedAt { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("repeat")]
        public string Repeat { get; set; } = "DAILY"; // "DAILY", "WEEKLY", "CUSTOM"
    }
}