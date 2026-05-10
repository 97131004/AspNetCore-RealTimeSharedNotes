using AspNetCore_RealTimeSharedNotes.Models;

namespace AspNetCore_RealTimeSharedNotes.Data.Interfaces;

public interface INotesRepository
{
    Task<List<Note>> GetAllNotesAsync();
    Task<Note?> GetNoteAsync(int noteId);
    Task<Note> AddNoteAsync(Note note);
    Task<bool> DeleteNoteAsync(int noteId);
}
