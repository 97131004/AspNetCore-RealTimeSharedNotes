using System.Security.Claims;
using AspNetCore_RealTimeSharedNotes.Models.Constants;
using AspNetCore_RealTimeSharedNotes.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCore_RealTimeSharedNotes.Hubs;

[Authorize]
public class NotesHub : Hub
{
    private readonly INotesService _noteService;
    private readonly IUsersService _userService;

    public NotesHub(INotesService noteService, IUsersService userService)
    {
        _noteService = noteService;
        _userService = userService;
    }

    //sends all existing notes to the newly connected client
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var notes = await _noteService.GetAllNotesAsync(userId);
        await Clients.Caller.SendAsync("LoadNotes", notes);
        await base.OnConnectedAsync();
    }

    //saves note to db and broadcasts delta to all clients
    public async Task AddNote(string content)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var role = Context.User?.FindFirstValue(ClaimTypes.Role)!;
        var note = await _noteService.AddNoteAsync(userId, role, content);
        //send isOwn=true to author, isOwn=false to everyone else
        var noteForOthers = note with { IsOwn = false };
        await Clients.AllExcept(Context.ConnectionId).SendAsync("NoteAdded", noteForOthers);
        await Clients.Caller.SendAsync("NoteAdded", note);
    }

    //deletes note from db and broadcasts removal delta to all clients
    public async Task DeleteNote(int noteId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var role = Context.User?.FindFirstValue(ClaimTypes.Role)!;
        var deleted = await _noteService.DeleteNoteAsync(noteId, userId, role);
        if (deleted)
            await Clients.All.SendAsync("NoteRemoved", noteId);
    }
}
