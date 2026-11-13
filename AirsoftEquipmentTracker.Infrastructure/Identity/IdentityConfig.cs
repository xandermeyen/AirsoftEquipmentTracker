using Microsoft.AspNetCore.Identity;

namespace AirsoftEquipmentTracker.Infrastructure.Identity;

// Wachtwoord- en aanmeldregels, gedeeld door de Blazor-app en de API.
// Zo kan een account dat via de API aangemaakt is ook inloggen in de Blazor-app, en omgekeerd.
public static class IdentityConfig
{
    public static void Configure(IdentityOptions options)
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    }
}
