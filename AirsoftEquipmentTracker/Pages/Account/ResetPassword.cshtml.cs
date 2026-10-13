using AirsoftEquipmentTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AirsoftEquipmentTracker.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ResetPasswordModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool ResetDone { get; set; }

    public class InputModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare(nameof(Password), ErrorMessage = "The passwords don't match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public IActionResult OnGet(string? token, string? email)
    {
        // Zonder token of e-mail is de link ongeldig: terug naar "wachtwoord vergeten"
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            return RedirectToPage("/Account/ForgotPassword");

        Input.Token = token;
        Input.Email = email;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _userManager.FindByEmailAsync(Input.Email);

        // Geen account: toon hetzelfde resultaat, zodat niemand kan afleiden
        // welke e-mailadressen geregistreerd zijn.
        if (user is null)
        {
            ResetDone = true;
            return Page();
        }

        var result = await _userManager.ResetPasswordAsync(user, Input.Token, Input.Password);

        if (result.Succeeded)
        {
            ResetDone = true;
            return Page();
        }

        foreach (var error in result.Errors)
        {
            // "Invalid token" is voor een gebruiker niet duidelijk
            var message = error.Code == "InvalidToken"
                ? "This reset link is invalid or has expired. Request a new one."
                : error.Description;
            ModelState.AddModelError(string.Empty, message);
        }

        return Page();
    }
}
