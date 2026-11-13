using AirsoftEquipmentTracker.Api.DTOs.Catalog;
using AirsoftEquipmentTracker.Core.Interfaces;
using AirsoftEquipmentTracker.Core.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirsoftEquipmentTracker.Api.Controllers;

// Brands zijn gedeeld door alle gebruikers, net als in de Blazor-app:
// iedereen mag ze opvragen en toevoegen, enkel een Admin mag hernoemen of verwijderen.
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class BrandsController(IEquipmentService equipmentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<BrandDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BrandDto>>> GetAll()
    {
        var items = await equipmentService.GetBrandsAsync();
        return Ok(items.Adapt<List<BrandDto>>());
    }

    // Bestaat de naam al, dan krijg je de bestaande terug (200) in plaats van een dubbele (201)
    [HttpPost]
    [ProducesResponseType<BrandDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<BrandDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BrandDto>> Create(CatalogNameRequestDto request)
    {
        var name = request.Name.Trim();
        if (name.Length == 0)
        {
            ModelState.AddModelError(nameof(request.Name), "Name is required.");
            return ValidationProblem(ModelState);
        }

        var existing = await FindByNameAsync(name);
        if (existing is not null)
            return Ok(existing.Adapt<BrandDto>());

        var created = await equipmentService.AddBrandAsync(name);
        return CreatedAtAction(nameof(GetAll), null, created.Adapt<BrandDto>());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Rename(int id, CatalogNameRequestDto request)
    {
        var name = request.Name.Trim();
        if (name.Length == 0)
        {
            ModelState.AddModelError(nameof(request.Name), "Name is required.");
            return ValidationProblem(ModelState);
        }

        // De service geeft enkel true/false terug; hier zoeken we uit waarom het zou mislukken
        var all = await equipmentService.GetBrandsAsync();
        if (!all.Any(x => x.Id == id))
            return NotFound(new { error = $"Brand {id} not found." });

        if (all.Any(x => x.Id != id && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
            return Conflict(new { error = $"A brand named '{name}' already exists." });

        await equipmentService.RenameBrandAsync(id, name);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var all = await equipmentService.GetBrandsAsync();
        if (!all.Any(x => x.Id == id))
            return NotFound(new { error = $"Brand {id} not found." });

        // Telt de items van alle gebruikers: een brand in gebruik mag niet weg
        var usage = await equipmentService.GetBrandUsageCountsAsync();
        if (usage.TryGetValue(id, out var count) && count > 0)
            return Conflict(new { error = $"This brand is still used by {count} item(s) and can't be deleted." });

        await equipmentService.DeleteBrandAsync(id);
        return NoContent();
    }

    private async Task<Brand?> FindByNameAsync(string name)
    {
        var all = await equipmentService.GetBrandsAsync();
        return all.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
