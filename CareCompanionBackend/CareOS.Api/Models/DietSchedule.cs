using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CareOS.Api.Models
{
    public class DietSchedule
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

        [BsonElement("mealType")]
        public string MealType { get; set; } = string.Empty; // "BREAKFAST", "LUNCH", "DINNER"

        [BsonElement("foodItems")]
        public List<string> FoodItems { get; set; } = new();

        [BsonElement("scheduledTime")]
        public string ScheduledTime { get; set; } = string.Empty; // "08:00 AM"

        [BsonElement("calories")]
        public int? Calories { get; set; }

        [BsonElement("isCompleted")]
        public bool IsCompleted { get; set; } = false;

        [BsonElement("completedAt")]
        public DateTime? CompletedAt { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;

        [BsonElement("notes")]
        public string? Notes { get; set; }
    }
}