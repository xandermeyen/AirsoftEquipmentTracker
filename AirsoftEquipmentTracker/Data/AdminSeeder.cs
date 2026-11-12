using AirsoftEquipmentTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace AirsoftEquipmentTracker.Data;

// Zorgt bij het opstarten dat de Admin-rol bestaat en geeft die aan het account
// uit de config-waarde "AdminEmail" (zet die in user-secrets, niet in appsettings.json).
public static class AdminSeeder
{
    public static async Task EnsureAdminAsync(IServiceProvider services, IConfiguration config, ILogger logger)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync(AppRoles.Admin))
            await roleManager.CreateAsync(new IdentityRole(AppRoles.Admin));

        var adminEmail = config["AdminEmail"];
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
