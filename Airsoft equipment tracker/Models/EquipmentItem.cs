namespace Airsoft_equipment_tracker.Models
{
    public class EquipmentItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int BrandId { get; set; }
        public Brand? Brand { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public decimal Price { get; set; }

        public DateTime PurchaseDate { get; set; }

        public string Notes { get; set; } = string.Empty;
    }
}
