using System.Security.Claims;
using System.Text;
using AirsoftEquipmentTracker.Api.Settings;
using AirsoftEquipmentTracker.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AirsoftEquipmentTracker.Api.Services;

// Maakt de JWT-tokens aan die de client meestuurt in de Authorization-header.
public class TokenService
{
    private readonly JwtSettings _settings;
    private readonly SigningCredentials _credentials;
    private readonly JsonWebTokenHandler _handler = new();

    public TokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public (string Token, DateTime Expiration) GenerateToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var expiration = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            // NameIdentifier is wat de controllers gebruiken om de ingelogde gebruiker op te halen
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Name, user.DisplayName ?? user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = _credentials
        };

        return (_handler.CreateToken(descriptor), expiration);
    }
}
