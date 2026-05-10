using Microsoft.AspNetCore.Identity;

namespace AspNetCore_RealTimeSharedNotes.Models;

//extends IdentityUserRole with role so ef can join AspNetUserRoles with AspNetRoles via ThenInclude without manual linq-to-sql
public class ApplicationUserRole : IdentityUserRole<string>
{
    public virtual IdentityRole Role { get; set; } = null!;
}
