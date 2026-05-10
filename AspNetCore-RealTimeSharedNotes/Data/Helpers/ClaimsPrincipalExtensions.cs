using System.Security.Claims;
using AspNetCore_RealTimeSharedNotes.Models.Constants;

namespace AspNetCore_RealTimeSharedNotes.Data.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static string GetRequestingRole(this ClaimsPrincipal user)
        => user.IsInRole(UserRoles.SuperAdmin) ? UserRoles.SuperAdmin : user.IsInRole(UserRoles.Admin) ? UserRoles.Admin : UserRoles.User;
}
