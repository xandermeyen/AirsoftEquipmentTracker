using System.ComponentModel.DataAnnotations;

namespace AirsoftEquipmentTracker.Models
{
    public class EquipmentItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(200, ErrorMessage = "Name can be at most 200 characters.")]
        public string Name { get; set; } = string.Empty;

        public int BrandId { get; set; }
        public Brand? Brand { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Range(0, 100000, ErrorMessage = "Price must be between 0 and 100,000.")]
        public decimal Price { get; set; }

        [NotInFuture(ErrorMessage = "Purchase date cannot be in the future.")]
        public DateTime PurchaseDate { get; set; }

        // Standaard Owned, zodat bestaande rijen na de migratie gewoon meetellen
        public EquipmentStatus Status { get; set; } = EquipmentStatus.Owned;

        // Relatief pad naar de foto in wwwroot (bv. "uploads/abc.jpg").
        // Null betekent: geen foto, toon de placeholder.
        [MaxLength(260)]
        public string? ImagePath { get; set; }

        [MaxLength(2000, ErrorMessage = "Notes can be at most 2000 characters.")]
        public string Notes { get; set; } = string.Empty;
    }
}
