using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace AirsoftEquipmentTracker.Api.Configuration;

// Vertelt het OpenAPI-document dat de API JWT-tokens gebruikt.
// Daardoor toont Scalar een veld om één keer je token in te vullen,
// dat daarna bij elke request meegestuurd wordt.
public class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste the token from POST /api/auth/login."
        };

        // Op documentniveau: geldt voor alle endpoints. Endpoints met [AllowAnonymous]
        // werken gewoon ook zonder token.
        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });

        return Task.CompletedTask;
    }
}
