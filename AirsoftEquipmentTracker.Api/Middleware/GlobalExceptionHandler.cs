using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace AirsoftEquipmentTracker.Api.Middleware;

// Vangt alle exceptions op die niet in een controller afgehandeld worden en zet ze om
// naar een nette JSON-response met de juiste statuscode. Zo is er geen try/catch nodig in elke actie.
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        // Onverwachte fouten loggen we volledig; de client krijgt geen interne details te zien
        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Onverwachte fout bij {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        string message = statusCode == StatusCodes.Status500InternalServerError
            ? "An unexpected error occurred."
            : exception.Message;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new { error = message }, cancellationToken);

        return true;
    }
}
