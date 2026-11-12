namespace AirsoftEquipmentTracker.Models;

// Cijfers voor het dashboard van één gebruiker.
// Totalen tellen alleen Owned items mee; de wishlist heeft een eigen totaal.
public record DashboardStats(
    int OwnedCount,
    decimal OwnedValue,
    decimal WishlistValue,
    int CategoriesUsed,
    List<EquipmentItem> RecentItems,
    List<CategoryCount> CategoryBreakdown)
{
    public static DashboardStats Empty { get; } = new(0, 0m, 0m, 0, new(), new());
}

public record CategoryCount(string Name, int Count);
