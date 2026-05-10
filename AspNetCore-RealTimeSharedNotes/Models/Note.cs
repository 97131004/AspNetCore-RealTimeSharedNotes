namespace AspNetCore_RealTimeSharedNotes.Models;

public class Note
{
    public int NoteId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ApplicationUser User { get; set; } = null!;
}
