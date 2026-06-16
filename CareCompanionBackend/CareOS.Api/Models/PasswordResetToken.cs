using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CareOS.Api.Models
{
    public class PasswordResetToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("resetCode")]
        public string ResetCode { get; set; } = string.Empty; // 6-digit code

        [BsonElement("isUsed")]
        public bool IsUsed { get; set; } = false;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("expiresAt")]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(15);
    }
}