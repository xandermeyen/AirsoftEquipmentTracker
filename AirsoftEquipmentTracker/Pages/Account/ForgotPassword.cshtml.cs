using AirsoftEquipmentTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AirsoftEquipmentTracker.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ForgotPasswordModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool EmailSent { get; set; }
    public string? SentTo { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // Always show the "check your email" screen regardless of whether the
        // account exists — this prevents user enumeration.
        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // TODO: send token via email (requires IEmailSender setup).
            // For local dev, log the reset link to the console:
            var resetLink = Url.Page("/Account/ResetPassword",
                pageHandler: null,
                values: new { token, email = Input.Email },
                protocol: Request.Scheme);

            Console.WriteLine($"[DEV] Password reset link for {Input.Email}: {resetLink}");
        }

        EmailSent = true;
        SentTo = Input.Email;
        return Page();
    }
}
