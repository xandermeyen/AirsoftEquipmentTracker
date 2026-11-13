using System.ComponentModel.DataAnnotations;

namespace AirsoftEquipmentTracker.Api.DTOs.Catalog;

// Request om een brand of categorie aan te maken of te hernoemen. Enkel een naam.
public class CatalogNameRequestDto
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200, ErrorMessage = "Name can be at most 200 characters.")]
    public string Name { get; set; } = string.Empty;
}
