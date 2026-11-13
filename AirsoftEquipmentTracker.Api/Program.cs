using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using AirsoftEquipmentTracker.Api.Configuration;
using AirsoftEquipmentTracker.Api.Middleware;
using AirsoftEquipmentTracker.Api.Services;
using AirsoftEquipmentTracker.Api.Settings;
using AirsoftEquipmentTracker.Infrastructure;
using AirsoftEquipmentTracker.Infrastructure.Data;
using AirsoftEquipmentTracker.Infrastructure.Identity;
using AirsoftEquipmentTracker.Infrastructure.Services;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options =>
        // Routes in kleine letters: /api/equipment in plaats van /api/Equipment
        options.Conventions.Add(new RouteTokenTransformerConvention(new LowercaseRouteTransformer())))
    .AddJsonOptions(options =>
        // Enums als tekst ("Wishlist") in plaats van als getal (2), leesbaarder voor een frontend
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Duidelijke 400-meldingen, ook als de JSON zelf ongeldig is (zie ValidationResponseFactory)
builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.InvalidModelStateResponseFactory = ValidationResponseFactory.Create);

// OpenAPI-document, met JWT zodat Scalar een veld voor het token toont
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

// Mapster: zoekt alle IRegister-klassen (zoals MapperProfile) in dit project
TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

// Exceptions worden centraal afgehandeld (zie GlobalExceptionHandler)
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Database en services, exact dezelfde als in de Blazor-app.
// Uploads:Path is relatief tegenover dit project; standaard wijst het naar wwwroot/uploads
// van de Blazor-app, zodat beide apps dezelfde foto's gebruiken.
var uploadsFolder = Path.GetFullPath(
    builder.Configuration["Uploads:Path"]
        ?? throw new InvalidOperationException("Uploads:Path ontbreekt in de configuratie."),
    builder.Environment.ContentRootPath);

builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' ontbreekt."),
    uploadsFolder);

// Identity zonder cookies: de API werkt enkel met JWT-tokens
builder.Services
    .AddIdentityCore<ApplicationUser>(IdentityConfig.Configure)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// JWT-instellingen: gebonden en gevalideerd bij het opstarten,
// zodat een ontbrekende of te korte secret meteen opvalt
builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
    .Validate(s => Encoding.UTF8.GetByteCount(s.Secret) >= 32,
              "Jwt:Secret moet minstens 32 bytes lang zijn (zet hem in user-secrets).")
    .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer) && !string.IsNullOrWhiteSpace(s.Audience),
              "Jwt:Issuer en Jwt:Audience zijn verplicht.")
    .ValidateOnStart();

builder.Services.AddSingleton<TokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// JwtBearer configureren met de gevalideerde JwtSettings
builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>>((options, settings) =>
    {
        var s = settings.Value;

        // Claims niet hernoemen naar de lange Microsoft-namen
        options.MapInboundClaims = false;
        options.IncludeErrorDetails = builder.Environment.IsDevelopment();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s.Secret)),
            ValidIssuer = s.Issuer,
            ValidAudience = s.Audience,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "name",
            RoleClaimType = ClaimTypes.Role
        };

        // Duidelijke JSON-foutmeldingen in plaats van een lege 401/403
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return context.Response.WriteAsJsonAsync(
                    new { error = "Not authenticated. The token is missing or invalid." });
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return context.Response.WriteAsJsonAsync(
                    new { error = "Access denied. You don't have the required role." });
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Admin-rol aanmaken en toekennen aan "AdminEmail", net als in de Blazor-app
try
{
    await AdminSeeder.EnsureAdminAsync(app.Services, app.Configuration["AdminEmail"], app.Logger);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Kon de Admin-rol niet instellen.");
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // OpenAPI-document op /openapi/v1.json en een testpagina op /scalar
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Foto's serveren op /uploads/..., uit dezelfde map als de Blazor-app
Directory.CreateDirectory(uploadsFolder);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsFolder),
    RequestPath = "/" + ImageService.UploadsFolderName
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
