using AspNetCore_RealTimeSharedNotes.Models;
using Microsoft.AspNetCore.Identity;

namespace AspNetCore_RealTimeSharedNotes.UnitTests;

public abstract class TestBase
{
    protected static ApplicationUser MakeUser(string id, string email, string role)
    {
        var user = new ApplicationUser
        {
            Id = id,
            Email = email,
            UserName = email
        };
        var userRole = new ApplicationUserRole
        {
            Role = new IdentityRole(role) { NormalizedName = role.ToUpper() }
        };
        user.UserRoles = [userRole];
        return user;
    }
}
