using System.Diagnostics;
using AspNetCore_RealTimeSharedNotes.Models;
using AspNetCore_RealTimeSharedNotes.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore_RealTimeSharedNotes.Controllers;

public class HomeController : BaseController
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public HomeController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    //redirect already-authenticated users straight to notes
    public IActionResult Index()
    {
        if (_signInManager.IsSignedIn(User))
            return RedirectToAction("Index", "Notes");
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Index", model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
        if (result.Succeeded)
            return RedirectToAction("Index", "Notes");

        ModelState.AddModelError(string.Empty, "invalid email or password");
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["SuccessMessage"] = "logged out successfully"; //lives until its read, so it will survive the redirect
        return RedirectToAction("Index");
    }

    public IActionResult Error()
    {
        return Content("error");
    }
}
