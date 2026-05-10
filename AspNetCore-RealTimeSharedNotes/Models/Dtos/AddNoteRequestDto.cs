namespace AspNetCore_RealTimeSharedNotes.Models.Dtos
{
    public record AddNoteRequestDto
    {
        public string Content { get; set; } = string.Empty;
    }
}
