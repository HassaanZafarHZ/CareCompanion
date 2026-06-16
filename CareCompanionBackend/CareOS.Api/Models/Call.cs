using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CareOS.Api.Models
{
    public class Call
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("callerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CallerId { get; set; } = string.Empty;

        [BsonElement("callerName")]
        public string CallerName { get; set; } = string.Empty;

        [BsonElement("receiverId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ReceiverId { get; set; } = string.Empty;

        [BsonElement("receiverName")]
        public string ReceiverName { get; set; } = string.Empty;

        [BsonElement("callType")]
        public string CallType { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = "INITIATED";

        [BsonElement("initiatedAt")]
        public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("acceptedAt")]
        public DateTime? AcceptedAt { get; set; }

        [BsonElement("endedAt")]
        public DateTime? EndedAt { get; set; }

        [BsonElement("durationSeconds")]
        public int DurationSeconds { get; set; } = 0;

        [BsonElement("endReason")]
        public string? EndReason { get; set; }
    }
}
