using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AirsoftEquipmentTracker.Api.Configuration;

// Bepaalt hoe een 400 bij ongeldige input eruitziet.
// Standaard geeft ASP.NET bij onleesbare JSON (bv. "Broken" als status) een technische
// melding met interne typenamen, plus een verwarrende "The request field is required.".
// Hier vervangen we dat door een korte melding per veld. Gewone validatiefouten
// (zoals "Name is required.") blijven ongewijzigd.
public static class ValidationResponseFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var modelState = context.ModelState;

        // Sleutels die met "$" beginnen komen van de JSON-parser, bv. "$.status"
        var jsonErrors = modelState
            .Where(entry => entry.Key.StartsWith('$') && entry.Value?.Errors.Count > 0)
            .Select(entry => entry.Key)
            .ToList();

        if (jsonErrors.Count > 0)
        {
            // Kon de JSON niet gelezen worden, dan zijn de andere fouten niet zinvol:
            // de body werd nooit echt gevalideerd. We tonen enkel de JSON-fouten.
            var friendly = new ModelStateDictionary();
            foreach (var key in jsonErrors)
            {
                var field = key.TrimStart('$', '.');
                if (field.Length == 0)
                    friendly.AddModelError("body", "The request body is not valid JSON.");
                else
                    friendly.AddModelError(field, $"The value for '{field}' is not valid.");
            }
            modelState = friendly;
        }

        var problemDetails = context.HttpContext.RequestServices
            .GetRequiredService<ProblemDetailsFactory>()
            .CreateValidationProblemDetails(context.HttpContext, modelState);

        return new BadRequestObjectResult(problemDetails)
        {
            ContentTypes = { "application/problem+json" }
        };
    }
}
