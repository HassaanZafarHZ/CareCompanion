namespace CareOS.Api.DTOs
{
    public class InitiateCallDto
    {
        public string CallerId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string CallType { get; set; } = "VOICE"; // VOICE, VIDEO
    }

    public class AcceptCallDto
    {
        public string CallId { get; set; } = string.Empty;
    }

    public class EndCallDto
    {
        public string CallId { get; set; } = string.Empty;
        public string EndReason { get; set; } = "COMPLETED";
    }

    public class CallResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string CallerId { get; set; } = string.Empty;
        public string CallerName { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string CallType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime InitiatedAt { get; set; }
        public int DurationSeconds { get; set; }
    }
}