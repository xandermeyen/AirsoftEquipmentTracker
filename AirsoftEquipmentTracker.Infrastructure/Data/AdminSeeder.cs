using AirsoftEquipmentTracker.Core.Models;
using AirsoftEquipmentTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AirsoftEquipmentTracker.Infrastructure.Data;

// Zorgt bij het opstarten dat de Admin-rol bestaat en geeft die aan het account met adminEmail.
// Die waarde komt uit de config ("AdminEmail" in user-secrets, niet in appsettings.json).
public static class AdminSeeder
{
    public static async Task EnsureAdminAsync(IServiceProvider services, string? adminEmail, ILogger logger)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync(AppRoles.Admin))
            await roleManager.CreateAsync(new IdentityRole(AppRoles.Admin));

        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            logger.LogWarning("Geen AdminEmail ingesteld. Niemand kan brands en categorieën beheren.");
            return;
        }

        // Bestaat het account nog niet, dan krijgt het de rol bij het registreren (zie RegisterModel)
        var user = await userManager.FindByEmailAsync(adminEmail);
        if (user is null)
            return;

        if (!await userManager.IsInRoleAsync(user, AppRoles.Admin))
            await userManager.AddToRoleAsync(user, AppRoles.Admin);
    }
}
