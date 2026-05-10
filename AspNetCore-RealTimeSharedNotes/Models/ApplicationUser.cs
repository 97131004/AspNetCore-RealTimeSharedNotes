using Microsoft.AspNetCore.Identity;

namespace AspNetCore_RealTimeSharedNotes.Models;

//only include what is actually used; unused Identity columns are suppressed in ApplicationDbContext
public class ApplicationUser : IdentityUser
{
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ApiKey? ApiKey { get; set; }
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = [];
}
