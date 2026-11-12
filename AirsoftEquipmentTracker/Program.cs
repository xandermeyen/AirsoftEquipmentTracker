using AirsoftEquipmentTracker.Components;
using AirsoftEquipmentTracker.Data;
using AirsoftEquipmentTracker.Models;
using AirsoftEquipmentTracker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Razor Pages needed for the auth pages (login, register, forgot password, logout)
builder.Services.AddRazorPages();

// AddDbContextFactory also registers AppDbContext as scoped (EF Core 5+),
// so Identity can resolve it without a separate AddDbContext call.
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Reset-links verlopen na 30 minuten (standaard is dat 1 dag)
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
    options.TokenLifespan = TimeSpan.FromMinutes(30));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

// CascadingAuthenticationState makes auth state available in all Blazor components
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddSingleton<EquipmentService>();
// ImageService krijgt alleen een map mee, zodat hij niet afhangt van ASP.NET zelf
builder.Services.AddSingleton(sp => new ImageService(
    Path.Combine(sp.GetRequiredService<IWebHostEnvironment>().WebRootPath, ImageService.UploadsFolderName)));

var app = builder.Build();

// Admin-rol aanmaken en toekennen aan het account uit "AdminEmail" (user-secrets).
// Mislukt dit (bv. database draait niet), dan start de app toch, maar loggen we het.
try
{
    await AdminSeeder.EnsureAdminAsync(app.Services, app.Configuration, app.Logger);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Kon de Admin-rol niet instellen.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Auth middleware must come before UseAntiforgery and routing
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.MapStaticAssets();

// MapStaticAssets kent alleen bestanden die bij build-time bestaan.
// UseStaticFiles is nodig om runtime geuploade foto's uit wwwroot/uploads te serveren.
app.UseStaticFiles();

app.MapRazorPages();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
