using System.Security.Claims;
using AspNetCore_RealTimeSharedNotes.Models.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore_RealTimeSharedNotes.Controllers;

[Authorize]
public class NotesController : BaseController
{
    public IActionResult Index()
    {
        PopulateViewData();
        return View();
    }
}
