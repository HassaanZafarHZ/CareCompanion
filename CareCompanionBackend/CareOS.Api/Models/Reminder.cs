using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CareOS.Api.Models
{
    public class Reminder
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? Type { get; set; } // Medication, Appointment, etc.
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DateTime { get; set; }
        public string? RepeatRule { get; set; } // e.g., Daily, Weekly
        public bool IsActive { get; set; } = true;
    }
}