using Airsoft_equipment_tracker.Models;

namespace Airsoft_equipment_tracker.Services;

public class MockEquipmentService
{
    private List<EquipmentItem> _equipment = new List<EquipmentItem>
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
            },

            new EquipmentItem
            {
                Id = 4,
                Name = "Red Dot Sight",
                Price = 90,
                PurchaseDate = new DateTime(2024, 2, 14),
                Brand = new Brand { Id = 4, Name = "Vector Optics" },
                Category = new Category { Id = 4, Name = "Attachment" },
                Notes = "Mounted on SSR90"
            },

            new EquipmentItem
            {
                Id = 5,
                Name = "Mid-Cap Magazine",
                Price = 15,
                PurchaseDate = new DateTime(2024, 3, 2),
                Brand = new Brand { Id = 5, Name = "Specna Arms" },
                Category = new Category { Id = 5, Name = "Magazine" },
                Notes = "120 round mid-cap"
            },

            new EquipmentItem
            {
                Id = 6,
                Name = "LiPo Battery 11.1V",
                Price = 35,
                PurchaseDate = new DateTime(2024, 4, 18),
                Brand = new Brand { Id = 6, Name = "Titan Power" },
                Category = new Category { Id = 6, Name = "Gear" },
                Notes = "Used for SSR90"
            },

            new EquipmentItem
            {
                Id = 7,
                Name = "Speedloader",
                Price = 8,
                PurchaseDate = new DateTime(2023, 11, 5),
                Brand = new Brand { Id = 7, Name = "Cyma" },
                Category = new Category { Id = 3, Name = "Gear" },
                Notes = "For loading magazines"
            },

            new EquipmentItem
            {
                Id = 8,
                Name = "0.28g BB Bottle",
                Price = 18,
                PurchaseDate = new DateTime(2024, 6, 1),
                Brand = new Brand { Id = 8, Name = "BLS" },
                Category = new Category { Id = 7, Name = "Consumable" },
                Notes = "Outdoor BBs"
            }

    };

    public List<EquipmentItem> GetEquipment()
    {
        return _equipment;
    }

    public void AddEquipment(EquipmentItem item)
    {
        item.Id = _equipment.Any() ? _equipment.Max(x => x.Id) + 1 : 1;
        _equipment.Add(item);
    }
}