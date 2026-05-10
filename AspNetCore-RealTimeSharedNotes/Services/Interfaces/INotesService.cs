using AspNetCore_RealTimeSharedNotes.Models.Dtos;

namespace AspNetCore_RealTimeSharedNotes.Services.Interfaces;

public interface INotesService
{
    Task<List<NoteDto>> GetAllNotesAsync(string? callerId = null);
    Task<NoteDto> AddNoteAsync(string userId, string role, string content);
    Task<bool> DeleteNoteAsync(int noteId, string requestingUserId, string requestingRole);
}
