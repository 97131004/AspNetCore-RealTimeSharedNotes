namespace AspNetCore_RealTimeSharedNotes.Models.Dtos;

public record NoteDto
{
    public int NoteId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public bool IsOwn { get; set; }
    public string AuthorRole { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
