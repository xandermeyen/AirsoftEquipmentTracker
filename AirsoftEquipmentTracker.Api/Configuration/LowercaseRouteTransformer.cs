namespace AirsoftEquipmentTracker.Api.Configuration;

// Zet [controller] in de routes om naar kleine letters,
// zodat URL's /api/equipment worden in plaats van /api/Equipment.
public class LowercaseRouteTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value) => value?.ToString()?.ToLowerInvariant();
}
