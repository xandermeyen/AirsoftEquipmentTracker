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

    public async Task AddEquipmentAsync(EquipmentItem item)
    {
        using var context = _contextFactory.CreateDbContext();
        item.Brand = null;
        item.Category = null;
        context.EquipmentItems.Add(item);
        await context.SaveChangesAsync();
    }
}