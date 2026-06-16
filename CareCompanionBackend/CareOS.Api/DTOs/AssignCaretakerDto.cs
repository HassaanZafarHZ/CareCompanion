namespace CareOS.Api.DTOs
{
    public class AssignCaretakerDto
    {
        public string ElderId { get; set; } = string.Empty;
        public string CaretakerId { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class AvailableCaretakerDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int AssignedEldersCount { get; set; }
        public bool IsAvailable { get; set; } // Max 3 elders se kam hai?
        public string? ProfilePicture { get; set; }
    }
}