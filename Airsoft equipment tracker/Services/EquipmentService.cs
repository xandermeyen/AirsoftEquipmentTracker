using Airsoft_equipment_tracker.Data;
using Airsoft_equipment_tracker.Models;
using Microsoft.EntityFrameworkCore;

namespace Airsoft_equipment_tracker.Services;

public class EquipmentService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public EquipmentService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<EquipmentItem>> GetEquipmentAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EquipmentItems
            .Include(e => e.Brand)
            .Include(e => e.Category)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<List<Brand>> GetBrandsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Brands
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    // Maakt een merk aan, of geeft het bestaande terug als de naam al bestaat.
    // SQL Server vergelijkt standaard hoofdletterongevoelig, dus "VFC" == "vfc".
    public async Task<Brand> AddBrandAsync(string name)
    {
        using var context = _contextFactory.CreateDbContext();
        var trimmed = name.Trim();

        var existing = await context.Brands.FirstOrDefaultAsync(b => b.Name == trimmed);
        if (existing is not null)
            return existing;

        var brand = new Brand { Name = trimmed };
        context.Brands.Add(brand);
        await context.SaveChangesAsync();
        return brand;
    }

    // Zelfde aanpak voor categorieen.
    public async Task<Category> AddCategoryAsync(string name)
    {
        using var context = _contextFactory.CreateDbContext();
        var trimmed = name.Trim();

        var existing = await context.Categories.FirstOrDefaultAsync(c => c.Name == trimmed);
        if (existing is not null)
            return existing;

        var category = new Category { Name = trimmed };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task AddEquipmentAsync(EquipmentItem item)
    {
        using var context = _contextFactory.CreateDbContext();
        item.Brand = null;
        item.Category = null;
        context.EquipmentItems.Add(item);
        await context.SaveChangesAsync();
    }

    // Haalt een enkel item op via id, inclusief Brand + Category.
    // Geeft null terug als het item niet bestaat (bv. ongeldige edit-url).
    public async Task<EquipmentItem?> GetEquipmentByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EquipmentItems
            .Include(e => e.Brand)
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task UpdateEquipmentAsync(EquipmentItem item)
    {
        using var context = _contextFactory.CreateDbContext();
        // Navigatie-props leegmaken zodat EF enkel via de foreign keys koppelt
        item.Brand = null;
        item.Category = null;
        context.EquipmentItems.Update(item);
        await context.SaveChangesAsync();
    }

    public async Task DeleteEquipmentAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var item = await context.EquipmentItems.FindAsync(id);

        // Niets te doen als het item al weg is
        if (item is null)
            return;

        context.EquipmentItems.Remove(item);
        await context.SaveChangesAsync();
    }

    // Telt per merk hoeveel equipment items ernaar verwijzen.
    // De beheerpagina gebruikt dit om delete te blokkeren met een nette melding.
    public async Task<Dictionary<int, int>> GetBrandUsageCountsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EquipmentItems
            .GroupBy(e => e.BrandId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
    }

    // Zelfde idee, maar per categorie.
    public async Task<Dictionary<int, int>> GetCategoryUsageCountsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EquipmentItems
            .GroupBy(e => e.CategoryId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
    }

    // Hernoemt een merk. Geeft false terug als het merk niet bestaat
    // of als de nieuwe naam al door een ander merk gebruikt wordt.
    public async Task<bool> RenameBrandAsync(int id, string newName)
    {
        using var context = _contextFactory.CreateDbContext();
        var brand = await context.Brands.FindAsync(id);
        if (brand is null)
            return false;

        var trimmed = newName.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return false;

        // Unieke index op Name, dus eerst checken op een duplicaat
        var duplicate = await context.Brands.AnyAsync(b => b.Id != id && b.Name == trimmed);
        if (duplicate)
            return false;

        brand.Name = trimmed;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RenameCategoryAsync(int id, string newName)
    {
        using var context = _contextFactory.CreateDbContext();
        var category = await context.Categories.FindAsync(id);
        if (category is null)
            return false;

        var trimmed = newName.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return false;

        var duplicate = await context.Categories.AnyAsync(c => c.Id != id && c.Name == trimmed);
        if (duplicate)
            return false;

        category.Name = trimmed;
        await context.SaveChangesAsync();
        return true;
    }

    // Verwijdert een merk, maar alleen als geen enkel item er nog naar verwijst.
    // De database heeft DeleteBehavior.Restrict, dus zonder deze check zou
    // SaveChanges een exception gooien.
    public async Task<bool> DeleteBrandAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();

        var inUse = await context.EquipmentItems.AnyAsync(e => e.BrandId == id);
        if (inUse)
            return false;

        var brand = await context.Brands.FindAsync(id);
        if (brand is null)
            return false;

        context.Brands.Remove(brand);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();

        var inUse = await context.EquipmentItems.AnyAsync(e => e.CategoryId == id);
        if (inUse)
            return false;

        var category = await context.Categories.FindAsync(id);
        if (category is null)
            return false;

        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        return true;
    }
}
