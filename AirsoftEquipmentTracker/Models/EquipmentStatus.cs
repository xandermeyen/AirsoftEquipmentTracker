namespace AirsoftEquipmentTracker.Models
{
    // Status van een equipment item.
    // Owned telt mee in de dashboard-totalen, Wishlist wordt apart getoond.
    public enum EquipmentStatus
    {
        Owned = 0,
        Sold = 1,
        Wishlist = 2
    }
}
