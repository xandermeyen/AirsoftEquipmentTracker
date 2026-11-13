using AirsoftEquipmentTracker.Core.Interfaces;
using AirsoftEquipmentTracker.Infrastructure.Data;
using AirsoftEquipmentTracker.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AirsoftEquipmentTracker.Infrastructure;

public static class DependencyInjection
{
    // Registreert database en services. Zowel de Blazor-app als straks de API
    // roepen dit aan, zodat de registratie maar op één plek staat.
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, string uploadsFolder)
    {
        // AddDbContextFactory registreert AppDbContext ook als scoped (EF Core 5+),
        // zodat Identity hem kan gebruiken zonder aparte AddDbContext.
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Geregistreerd op de interface, zodat Blazor en de API ervan afhangen en niet van de implementatie
        services.AddSingleton<IEquipmentService, EquipmentService>();

        // ImageService krijgt alleen een map mee, zodat hij niet afhangt van ASP.NET zelf
        services.AddSingleton<IImageService>(new ImageService(uploadsFolder));

        return services;
    }
}
