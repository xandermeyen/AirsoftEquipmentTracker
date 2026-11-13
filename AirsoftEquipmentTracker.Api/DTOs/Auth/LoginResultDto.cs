namespace AirsoftEquipmentTracker.Api.DTOs.Auth;

// Wat de client terugkrijgt na een geslaagde login. Expiration is in UTC.
public record LoginResultDto(string Token, DateTime Expiration);
