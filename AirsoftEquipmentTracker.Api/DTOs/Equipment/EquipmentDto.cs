using AirsoftEquipmentTracker.Core.Models;

namespace AirsoftEquipmentTracker.Api.DTOs.Equipment;

// Wat de API teruggeeft voor een equipment item.
// Bevat bewust geen UserId: de client hoeft niet te weten hoe eigenaars opgeslagen worden.
public class EquipmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public DateTime PurchaseDate { get; set; }
    public EquipmentStatus Status { get; set; }

    // Relatief pad naar de foto (bv. "uploads/abc.jpg"), of null als er geen foto is
    public string? ImagePath { get; set; }

    public string Notes { get; set; } = string.Empty;
}
