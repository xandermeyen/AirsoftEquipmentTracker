using Airsoft_equipment_tracker.Models;

namespace Airsoft_equipment_tracker.Services;

public class MockEquipmentService
{
    public List<EquipmentItem> GetEquipment()
    {
        return new List<EquipmentItem>
        {
            new EquipmentItem
            {
                Id = 1,
                Name = "SSR90",
                Price = 549,
                PurchaseDate = new DateTime(2024, 5, 10),
                Brand = new Brand { Id = 1, Name = "Novritsch" },
                Category = new Category { Id = 1, Name = "Primary" },
                Notes = "Main outdoor replica"
            },
            new EquipmentItem
            {
                Id = 2,
                Name = "Glock 17",
                Price = 180,
                PurchaseDate = new DateTime(2023, 8, 12),
                Brand = new Brand { Id = 2, Name = "Tokyo Marui" },
                Category = new Category { Id = 2, Name = "Secondary" },
                Notes = "Sidearm"
            },
            new EquipmentItem
            {
                Id = 3,
                Name = "Plate Carrier",
                Price = 120,
                PurchaseDate = new DateTime(2024, 1, 20),
                Brand = new Brand { Id = 3, Name = "Invader Gear" },
                Category = new Category { Id = 3, Name = "Gear" },
                Notes = "Main carrier setup"
            }
        };
    }
}