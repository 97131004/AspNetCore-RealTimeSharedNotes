using AspNetCore_RealTimeSharedNotes.Data.Helpers;
using AspNetCore_RealTimeSharedNotes.Models.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AspNetCore_RealTimeSharedNotes.Controllers;

public abstract class BaseController : Controller
{
    protected void PopulateViewData()
    {
        ViewData["UserId"] = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        ViewData["UserRole"] = User.GetRequestingRole();
    }
}
