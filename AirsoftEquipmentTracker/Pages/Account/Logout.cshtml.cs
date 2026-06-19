using AirsoftEquipmentTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AirsoftEquipmentTracker.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LogoutModel(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/Account/Login");
    }

    // Redirect GET requests (e.g. direct URL visits) back to home
    public IActionResult OnGet() => Redirect("/");
}
