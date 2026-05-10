using AspNetCore_RealTimeSharedNotes.Data.Interfaces;
using AspNetCore_RealTimeSharedNotes.Models;
using AspNetCore_RealTimeSharedNotes.Models.Constants;
using AspNetCore_RealTimeSharedNotes.Models.Dtos;
using AspNetCore_RealTimeSharedNotes.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AspNetCore_RealTimeSharedNotes.Services;

public class NotesService : INotesService
{
    private readonly INotesRepository _repo;

    public NotesService(INotesRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<NoteDto>> GetAllNotesAsync(string? callerId = null)
    {
        var notes = await _repo.GetAllNotesAsync();
        return notes.Select(n => ToDto(n, n.User?.UserRoles.FirstOrDefault()?.Role.Name ?? UserRoles.User, callerId)).ToList();
    }

    public async Task<NoteDto> AddNoteAsync(string userId, string role, string content)
    {
        var trimmedContent = content.Trim();
        if (trimmedContent.Length > ModelConstants.NoteContentMaxLength)
            trimmedContent = trimmedContent.Substring(0, ModelConstants.NoteContentMaxLength);

        var note = new Note { UserId = userId, Content = trimmedContent, CreatedAt = DateTime.UtcNow };
        var savedNote = await _repo.AddNoteAsync(note);
        return ToDto(savedNote, role, userId);
    }

    //user: own notes only | admin: own + any user's notes | superadmin: all notes
    public async Task<bool> DeleteNoteAsync(int noteId, string requestingUserId, string requestingRole)
    {
        var note = await _repo.GetNoteAsync(noteId);
        if (note == null)
            return false;

        if (requestingRole == UserRoles.SuperAdmin)
            return await _repo.DeleteNoteAsync(noteId);

        if (note.UserId == requestingUserId)
            return await _repo.DeleteNoteAsync(noteId);

        if (requestingRole == UserRoles.Admin)
        {
            var authorRole = note.User?.UserRoles.FirstOrDefault()?.Role.Name ?? UserRoles.User;
            if (authorRole == UserRoles.User)
                return await _repo.DeleteNoteAsync(noteId);
        }

        return false;
    }

    private static NoteDto ToDto(Note n, string authorRole, string? callerId) => new()
    {
        NoteId = n.NoteId,
        Content = n.Content,
        UserEmail = n.User?.Email ?? string.Empty,
        IsOwn = n.UserId == callerId,
        AuthorRole = authorRole,
        CreatedAt = n.CreatedAt
    };
}

