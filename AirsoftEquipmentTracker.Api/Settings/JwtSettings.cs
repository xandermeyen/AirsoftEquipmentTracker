namespace AirsoftEquipmentTracker.Api.Settings;

// Instellingen voor de JWT-tokens, uit de sectie "Jwt" in appsettings.json.
// Secret staat NIET in appsettings.json maar in user-secrets (Jwt:Secret).
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;

    // Wie het token uitgeeft en voor wie het bedoeld is. Bewust een naam en geen URL,
    // zodat de tokens niet afhangen van de poort waarop de API draait.
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 120;
}
