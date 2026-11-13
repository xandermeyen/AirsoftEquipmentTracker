using AirsoftEquipmentTracker.Api.DTOs.Catalog;
using AirsoftEquipmentTracker.Core.Interfaces;
using AirsoftEquipmentTracker.Core.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirsoftEquipmentTracker.Api.Controllers;

// Categories zijn gedeeld door alle gebruikers, net als in de Blazor-app:
// iedereen mag ze opvragen en toevoegen, enkel een Admin mag hernoemen of verwijderen.
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController(IEquipmentService equipmentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<CategoryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var items = await equipmentService.GetCategoriesAsync();
        return Ok(items.Adapt<List<CategoryDto>>());
    }

    // Bestaat de naam al, dan krijg je de bestaande terug (200) in plaats van een dubbele (201)
    [HttpPost]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDto>> Create(CatalogNameRequestDto request)
    {
        var name = request.Name.Trim();
        if (name.Length == 0)
        {
            ModelState.AddModelError(nameof(request.Name), "Name is required.");
            return ValidationProblem(ModelState);
        }

        var existing = await FindByNameAsync(name);
        if (existing is not null)
            return Ok(existing.Adapt<CategoryDto>());

        var created = await equipmentService.AddCategoryAsync(name);
        return CreatedAtAction(nameof(GetAll), null, created.Adapt<CategoryDto>());
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
        var all = await equipmentService.GetCategoriesAsync();
        if (!all.Any(x => x.Id == id))
            return NotFound(new { error = $"Category {id} not found." });

        if (all.Any(x => x.Id != id && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
            return Conflict(new { error = $"A category named '{name}' already exists." });

        await equipmentService.RenameCategoryAsync(id, name);
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
        var all = await equipmentService.GetCategoriesAsync();
        if (!all.Any(x => x.Id == id))
            return NotFound(new { error = $"Category {id} not found." });

        // Telt de items van alle gebruikers: een category in gebruik mag niet weg
        var usage = await equipmentService.GetCategoryUsageCountsAsync();
        if (usage.TryGetValue(id, out var count) && count > 0)
            return Conflict(new { error = $"This category is still used by {count} item(s) and can't be deleted." });

        await equipmentService.DeleteCategoryAsync(id);
        return NoContent();
    }

    private async Task<Category?> FindByNameAsync(string name)
    {
        var all = await equipmentService.GetCategoriesAsync();
        return all.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
