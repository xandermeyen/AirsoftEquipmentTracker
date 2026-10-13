using AirsoftEquipmentTracker.Data;
using AirsoftEquipmentTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace AirsoftEquipmentTracker.Services;

public class EquipmentService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public EquipmentService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<EquipmentItem>> GetEquipmentAsync(string userId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EquipmentItems
            .Where(e => e.UserId == userId)
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

    // UserId is included in the filter so a user can never read another user's item.
    public async Task<EquipmentItem?> GetEquipmentByIdAsync(int id, string userId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EquipmentItems
            .Include(e => e.Brand)
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
    }

    public async Task UpdateEquipmentAsync(EquipmentItem item)
    {
        using var context = _contextFactory.CreateDbContext();
        item.Brand = null;
        item.Category = null;
        context.EquipmentItems.Update(item);
        await context.SaveChangesAsync();
    }

    public async Task DeleteEquipmentAsync(int id, string userId)
    {
        using var context = _contextFactory.CreateDbContext();
        var item = await context.EquipmentItems
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (item is null)
            return;

        context.EquipmentItems.Remove(item);
        await context.SaveChangesAsync();
    }

    public async Task<Dictionary<int, int>> GetBrandUsageCountsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EquipmentItems
            .GroupBy(e => e.BrandId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
    }

    public async Task<Dictionary<int, int>> GetCategoryUsageCountsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EquipmentItems
            .GroupBy(e => e.CategoryId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
    }

    public async Task<bool> RenameBrandAsync(int id, string newName)
    {
        using var context = _contextFactory.CreateDbContext();
        var brand = await context.Brands.FindAsync(id);
        if (brand is null)
            return false;

        var trimmed = newName.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return false;

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
