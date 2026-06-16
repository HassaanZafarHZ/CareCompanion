using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CareOS.Api.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("fullName")]
        public string FullName { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("pin")]
        public string? Pin { get; set; } // Elder ke liye 4-digit PIN

        [BsonElement("role")]
        public string Role { get; set; } = string.Empty; // "ELDER" ya "CARETAKER"

        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [BsonElement("dateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [BsonElement("profilePicture")]
        public string? ProfilePicture { get; set; }

        [BsonElement("bloodPressure")]
        public BloodPressureReading? CurrentBP { get; set; }

        [BsonElement("emergencyContact")]
        public string? EmergencyContact { get; set; }

        [BsonElement("medicalHistory")]
        public List<string> MedicalHistory { get; set; } = new();

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("lastLogin")]
        public DateTime? LastLogin { get; set; }
    }

    // Blood Pressure tracking ke liye nested class
    public class BloodPressureReading
    {
        public int Systolic { get; set; } // Upper reading
        public int Diastolic { get; set; } // Lower reading
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = string.Empty; // "Normal", "High", "Low"
    }
}