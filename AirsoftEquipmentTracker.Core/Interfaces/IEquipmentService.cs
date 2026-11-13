using AirsoftEquipmentTracker.Core.Models;

namespace AirsoftEquipmentTracker.Core.Interfaces;

// Contract voor alle data-toegang van equipment, brands en categorieën.
// De Blazor-app en de API hangen af van deze interface, niet van de implementatie,
// zodat controllers en componenten getest kunnen worden met een mock (Moq).
public interface IEquipmentService
{
    // Equipment (altijd beperkt tot de items van één gebruiker)
    Task<List<EquipmentItem>> GetEquipmentAsync(string userId);
    Task<EquipmentItem?> GetEquipmentByIdAsync(int id, string userId);
    Task AddEquipmentAsync(EquipmentItem item);
    Task<bool> UpdateEquipmentAsync(EquipmentItem item, string userId);
    Task DeleteEquipmentAsync(int id, string userId);
    Task<DashboardStats> GetDashboardStatsAsync(string userId);

    // Brands en categorieën (gedeeld door alle gebruikers)
    Task<List<Brand>> GetBrandsAsync();
    Task<List<Category>> GetCategoriesAsync();
    Task<Brand> AddBrandAsync(string name);
    Task<Category> AddCategoryAsync(string name);
    Task<bool> RenameBrandAsync(int id, string newName);
    Task<bool> RenameCategoryAsync(int id, string newName);
    Task<bool> DeleteBrandAsync(int id);
    Task<bool> DeleteCategoryAsync(int id);
    Task<Dictionary<int, int>> GetBrandUsageCountsAsync();
    Task<Dictionary<int, int>> GetCategoryUsageCountsAsync();
}
