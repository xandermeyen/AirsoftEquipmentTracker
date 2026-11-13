using System.Security.Claims;
using AirsoftEquipmentTracker.Api.DTOs.Auth;
using AirsoftEquipmentTracker.Api.Services;
using AirsoftEquipmentTracker.Core.Models;
using AirsoftEquipmentTracker.Infrastructure.Data;
using AirsoftEquipmentTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AirsoftEquipmentTracker.Api.Controllers;

// Registreren, inloggen (geeft een JWT terug) en de ingelogde gebruiker opvragen.
// [ApiController] valideert de DTO's automatisch en geeft een 400 terug bij fouten,
// dus een ModelState.IsValid-check in elke actie is niet nodig.
[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    TokenService tokenService,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            ModelState.AddModelError(nameof(request.Email), "An account with this email already exists.");
            return ValidationProblem(ModelState);
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return ValidationProblem(ModelState);
        }

        // Zelfde regel als in de Blazor-app: het account uit "AdminEmail" krijgt meteen de Admin-rol
        if (AdminSeeder.IsAdminEmail(configuration["AdminEmail"], request.Email))
            await userManager.AddToRoleAsync(user, AppRoles.Admin);

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResultDto>> Login(LoginDto request)
    {
        // Altijd dezelfde melding, zodat niemand kan uitzoeken welke e-mailadressen bestaan
        var invalidLogin = Unauthorized(new { error = "Invalid email or password." });

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return invalidLogin;

        // lockoutOnFailure: na te veel foute pogingen wordt het account tijdelijk geblokkeerd
        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
            return Unauthorized(new { error = "Too many failed attempts. Try again in a few minutes." });

        if (!result.Succeeded)
            return invalidLogin;

        var roles = await userManager.GetRolesAsync(user);
        var (token, expiration) = tokenService.GenerateToken(user, roles);

        return Ok(new LoginResultDto(token, expiration));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<CurrentUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserDto>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = userId is null ? null : await userManager.FindByIdAsync(userId);

        // Token is geldig, maar het account bestaat niet meer (bv. verwijderd)
        if (user is null)
            return Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new CurrentUserDto(user.Id, user.Email ?? string.Empty, user.DisplayName, roles));
    }
}
