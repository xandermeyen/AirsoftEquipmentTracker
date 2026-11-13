namespace AirsoftEquipmentTracker.Api.DTOs.Auth;

// Gegevens van de ingelogde gebruiker, bv. om in een frontend de naam en rol te tonen.
public record CurrentUserDto(string Id, string Email, string? DisplayName, IList<string> Roles);
