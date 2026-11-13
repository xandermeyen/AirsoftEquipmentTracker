using System.Security.Claims;
using AirsoftEquipmentTracker.Api.DTOs.Equipment;
using AirsoftEquipmentTracker.Core.Interfaces;
using AirsoftEquipmentTracker.Core.Models;
using AirsoftEquipmentTracker.Infrastructure.Services;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirsoftEquipmentTracker.Api.Controllers;

// CRUD voor de equipment items van de ingelogde gebruiker.
// Alle acties werken enkel op de eigen items: de UserId komt altijd uit het token,
// nooit uit de request. Een item van iemand anders geeft dus gewoon 404.
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class EquipmentController(IEquipmentService equipmentService, IImageService imageService) : ControllerBase
{
    // [Authorize] garandeert een geldig token, en ons token bevat altijd deze claim
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("The token has no user id.");

    // GET api/equipment of api/equipment?status=Wishlist
    [HttpGet]
    [ProducesResponseType<List<EquipmentDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EquipmentDto>>> GetAll([FromQuery] EquipmentStatus? status)
    {
        var items = await equipmentService.GetEquipmentAsync(UserId);

        if (status is not null)
            items = items.Where(item => item.Status == status).ToList();

        // Een lege lijst is geen fout: gewoon 200 met []
        return Ok(items.Adapt<List<EquipmentDto>>());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<EquipmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipmentDto>> GetById(int id)
    {
        var item = await equipmentService.GetEquipmentByIdAsync(id, UserId);
        if (item is null)
            return NotFound(new { error = $"Equipment item {id} not found." });

        return Ok(item.Adapt<EquipmentDto>());
    }

    [HttpPost]
    [ProducesResponseType<EquipmentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EquipmentDto>> Create(EquipmentRequestDto request)
    {
        if (!await BrandAndCategoryExistAsync(request))
            return ValidationProblem(ModelState);

        var item = request.Adapt<EquipmentItem>();
        item.UserId = UserId;
        await equipmentService.AddEquipmentAsync(item);

        // Opnieuw ophalen, zodat BrandName en CategoryName ingevuld zijn in het antwoord
        var created = await equipmentService.GetEquipmentByIdAsync(item.Id, UserId);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, created!.Adapt<EquipmentDto>());
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, EquipmentRequestDto request)
    {
        var item = await equipmentService.GetEquipmentByIdAsync(id, UserId);
        if (item is null)
            return NotFound(new { error = $"Equipment item {id} not found." });

        if (!await BrandAndCategoryExistAsync(request))
            return ValidationProblem(ModelState);

        // Mapt de DTO op het bestaande item. Id, UserId en ImagePath blijven
        // ongewijzigd (zie MapperProfile).
        request.Adapt(item);
        await equipmentService.UpdateEquipmentAsync(item, UserId);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await equipmentService.GetEquipmentByIdAsync(id, UserId);
        if (item is null)
            return NotFound(new { error = $"Equipment item {id} not found." });

        await equipmentService.DeleteEquipmentAsync(id, UserId);

        // Foto pas weg nadat het item verwijderd is, net als in de Blazor-app
        imageService.DeleteImage(item.ImagePath);

        return NoContent();
    }

    // Foto uploaden of vervangen. Verwacht multipart/form-data met een veld "file".
    // De foto is daarna bereikbaar op /{imagePath}, bv. /uploads/abc.jpg.
    [HttpPost("{id:int}/image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<EquipmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipmentDto>> UploadImage(int id, IFormFile file)
    {
        var item = await equipmentService.GetEquipmentByIdAsync(id, UserId);
        if (item is null)
            return NotFound(new { error = $"Equipment item {id} not found." });

        var imageError = ImageService.Validate(file.FileName, file.Length);
        if (imageError is not null)
        {
            ModelState.AddModelError(nameof(file), imageError);
            return ValidationProblem(ModelState);
        }

        // Zelfde volgorde als in de Blazor-app: nieuwe foto eerst bewaren,
        // de oude pas verwijderen nadat het item zelf opgeslagen is.
        var oldImage = item.ImagePath;
        string newImage;
        await using (var stream = file.OpenReadStream())
        {
            newImage = await imageService.SaveImageAsync(stream, file.FileName, file.Length);
        }

        try
        {
            item.ImagePath = newImage;
            await equipmentService.UpdateEquipmentAsync(item, UserId);
        }
        catch
        {
            // Opslaan mislukt: nieuwe foto opruimen, de oude blijft gewoon staan
            imageService.DeleteImage(newImage);
            throw;
        }

        imageService.DeleteImage(oldImage);

        var updated = await equipmentService.GetEquipmentByIdAsync(id, UserId);
        return Ok(updated!.Adapt<EquipmentDto>());
    }

    [HttpDelete("{id:int}/image")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(int id)
    {
        var item = await equipmentService.GetEquipmentByIdAsync(id, UserId);
        if (item is null)
            return NotFound(new { error = $"Equipment item {id} not found." });

        var oldImage = item.ImagePath;
        if (oldImage is null)
            return NoContent();

        item.ImagePath = null;
        await equipmentService.UpdateEquipmentAsync(item, UserId);
        imageService.DeleteImage(oldImage);

        return NoContent();
    }

    // Een onbestaande BrandId of CategoryId zou anders pas in de database falen,
    // met een 500 als gevolg. Zo krijgt de client een duidelijke 400.
    private async Task<bool> BrandAndCategoryExistAsync(EquipmentRequestDto request)
    {
        var brands = await equipmentService.GetBrandsAsync();
        if (!brands.Any(brand => brand.Id == request.BrandId))
            ModelState.AddModelError(nameof(request.BrandId), "This brand does not exist.");

        var categories = await equipmentService.GetCategoriesAsync();
        if (!categories.Any(category => category.Id == request.CategoryId))
            ModelState.AddModelError(nameof(request.CategoryId), "This category does not exist.");

        return ModelState.IsValid;
    }
}
