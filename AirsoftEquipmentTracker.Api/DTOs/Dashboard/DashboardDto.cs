using AirsoftEquipmentTracker.Api.DTOs.Equipment;

namespace AirsoftEquipmentTracker.Api.DTOs.Dashboard;

// Cijfers voor het dashboard van de ingelogde gebruiker.
// Totalen tellen enkel Owned items; de wishlist heeft een eigen totaal.
public class DashboardDto
{
    public int OwnedCount { get; set; }
    public decimal OwnedValue { get; set; }
    public decimal WishlistValue { get; set; }
    public int CategoriesUsed { get; set; }

    // De 3 meest recent gekochte Owned items
    public List<EquipmentDto> RecentItems { get; set; } = [];

    public List<CategoryCountDto> CategoryBreakdown { get; set; } = [];
}

public class CategoryCountDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}
