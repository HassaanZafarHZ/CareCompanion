using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CareOS.Api.Models
{
    public class CheckIn
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public DateTime Date { get; set; }
        public DateTime Timestamp { get; set; }
            public string? Id { get; set; }
            public string? UserId { get; set; }
            public string? Status { get; set; } // e.g., "OK"
    }
}