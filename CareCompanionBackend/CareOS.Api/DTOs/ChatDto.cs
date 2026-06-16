namespace CareOS.Api.DTOs
{
    public class SendMessageDto
    {
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public string MessageType { get; set; } = "TEXT"; // "TEXT", "VOICE"
        public string? VoiceUrl { get; set; }
    }

    public class ChatResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty;
        public string? VoiceUrl { get; set; }
        public string? DetectedMood { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
    }
}