using System.ComponentModel.DataAnnotations;
using AirsoftEquipmentTracker.Core.Models;

namespace AirsoftEquipmentTracker.Api.DTOs.Equipment;

// Wat de client meestuurt bij aanmaken (POST) en aanpassen (PUT).
// Eén DTO voor beide, omdat de velden identiek zijn. Id, UserId en ImagePath
// staan er bewust niet in: die bepaalt de server, nooit de client.
public class EquipmentRequestDto
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200, ErrorMessage = "Name can be at most 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Choose a brand.")]
    public int BrandId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a category.")]
    public int CategoryId { get; set; }

    [Range(0, 100000, ErrorMessage = "Price must be between 0 and 100,000.")]
    public decimal Price { get; set; }

    [NotInFuture(ErrorMessage = "Purchase date cannot be in the future.")]
    public DateTime PurchaseDate { get; set; }

    // EnumDataType weigert getallen die geen geldige status zijn (bv. 99)
    [EnumDataType(typeof(EquipmentStatus), ErrorMessage = "Status must be Owned, Sold or Wishlist.")]
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Owned;

    [MaxLength(2000, ErrorMessage = "Notes can be at most 2000 characters.")]
    public string Notes { get; set; } = string.Empty;
}
