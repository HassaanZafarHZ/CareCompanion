namespace CareOS.Api.DTOs
{
    public class CreateNoteDto
    {
        public string ElderId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = "GENERAL";
    }

    public class UpdateNoteDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = "GENERAL";
    }
}