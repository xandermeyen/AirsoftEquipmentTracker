using Airsoft_equipment_tracker.Models;
using Airsoft_equipment_tracker.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Airsoft_equipment_tracker.Tests;

// Tests voor EquipmentService op een SQLite in-memory database.
// xUnit maakt per test een nieuwe instantie van deze klasse,
// dus elke test start met een verse database (alleen seed-data).
public class EquipmentServiceTests : IDisposable
{
    private readonly TestDbContextFactory _factory;
    private readonly EquipmentService _service;

    public EquipmentServiceTests()
    {
        _factory = new TestDbContextFactory();
        _service = new EquipmentService(_factory);
    }

    public void Dispose() => _factory.Dispose();

    // Hulpje: een geldig nieuw item dat naar seed-merk 1 en seed-categorie 1 wijst
    private static EquipmentItem NewItem(string name = "Test Replica") => new()
    {
        Name = name,
        BrandId = 1,
        CategoryId = 1,
        Price = 99.95m,
        PurchaseDate = new DateTime(2025, 1, 15),
        Notes = "Test item"
    };

    // ---------- Add / edit / delete ----------

    [Fact]
    public async Task AddEquipment_SavesItem_AndLoadsBrandAndCategory()
    {
        var item = NewItem();

        await _service.AddEquipmentAsync(item);

        var saved = await _service.GetEquipmentByIdAsync(item.Id);
        Assert.NotNull(saved);
        Assert.Equal("Test Replica", saved.Name);
        Assert.Equal(99.95m, saved.Price);
        Assert.NotNull(saved.Brand);      // Include werkt
        Assert.NotNull(saved.Category);
    }

    [Fact]
    public async Task UpdateEquipment_PersistsChanges()
    {
        var item = NewItem();
        await _service.AddEquipmentAsync(item);

        var toEdit = await _service.GetEquipmentByIdAsync(item.Id);
        toEdit!.Name = "Renamed Replica";
        toEdit.Price = 150m;
        toEdit.Status = EquipmentStatus.Sold;
        await _service.UpdateEquipmentAsync(toEdit);

        var updated = await _service.GetEquipmentByIdAsync(item.Id);
        Assert.NotNull(updated);
        Assert.Equal("Renamed Replica", updated.Name);
        Assert.Equal(150m, updated.Price);
        Assert.Equal(EquipmentStatus.Sold, updated.Status);
    }

    [Fact]
    public async Task DeleteEquipment_RemovesItem()
    {
        var item = NewItem();
        await _service.AddEquipmentAsync(item);

        await _service.DeleteEquipmentAsync(item.Id);

        var deleted = await _service.GetEquipmentByIdAsync(item.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteEquipment_UnknownId_DoesNotThrow()
    {
        // Mag gewoon niets doen, geen exception
        await _service.DeleteEquipmentAsync(999_999);
    }

    [Fact]
    public async Task GetEquipmentById_UnknownId_ReturnsNull()
    {
        var result = await _service.GetEquipmentByIdAsync(999_999);
        Assert.Null(result);
    }

    // ---------- AddBrandAsync dedupe ----------

    [Fact]
    public async Task AddBrand_CreatesNewBrand()
    {
        var brand = await _service.AddBrandAsync("Krytac");

        Assert.True(brand.Id > 0);
        Assert.Equal("Krytac", brand.Name);
    }

    [Fact]
    public async Task AddBrand_SameName_ReturnsExistingBrand()
    {
        var first = await _service.AddBrandAsync("Krytac");
        var second = await _service.AddBrandAsync("Krytac");

        Assert.Equal(first.Id, second.Id);

        var brands = await _service.GetBrandsAsync();
        Assert.Single(brands, b => b.Name == "Krytac");
    }

    [Fact]
    public async Task AddBrand_TrimsWhitespace()
    {
        var first = await _service.AddBrandAsync("Krytac");
        var second = await _service.AddBrandAsync("  Krytac  ");

        Assert.Equal(first.Id, second.Id);
        Assert.Equal("Krytac", second.Name);
    }

    // Let op: in productie (SQL Server) is de naamvergelijking hoofdletterongevoelig
    // door de databasecollatie. SQLite vergelijkt hoofdlettergevoelig, dus dat
    // gedrag kan hier niet 1-op-1 getest worden.

    [Fact]
    public async Task AddCategory_SameName_ReturnsExistingCategory()
    {
        var first = await _service.AddCategoryAsync("Sniper");
        var second = await _service.AddCategoryAsync("Sniper");

        Assert.Equal(first.Id, second.Id);
    }

    // ---------- Restrict-delete gedrag ----------

    [Fact]
    public async Task DeleteBrand_InUse_ReturnsFalse_AndKeepsBrand()
    {
        // Seed-merk 1 (Novritsch) wordt gebruikt door seed-item 1
        var result = await _service.DeleteBrandAsync(1);

        Assert.False(result);
        var brands = await _service.GetBrandsAsync();
        Assert.Contains(brands, b => b.Id == 1);
    }

    [Fact]
    public async Task DeleteBrand_Unused_ReturnsTrue_AndRemovesBrand()
    {
        var brand = await _service.AddBrandAsync("UnusedBrand");

        var result = await _service.DeleteBrandAsync(brand.Id);

        Assert.True(result);
        var brands = await _service.GetBrandsAsync();
        Assert.DoesNotContain(brands, b => b.Id == brand.Id);
    }

    [Fact]
    public async Task DeleteCategory_InUse_ReturnsFalse()
    {
        // Seed-categorie 1 (Primary) wordt gebruikt door seed-item 1
        var result = await _service.DeleteCategoryAsync(1);

        Assert.False(result);
    }

    [Fact]
    public async Task Database_RestrictsDeletingReferencedBrand()
    {
        // Rechtstreeks in de database een merk verwijderen dat nog
        // gebruikt wordt, moet falen door DeleteBehavior.Restrict.
        using var context = _factory.CreateDbContext();
        var brand = await context.Brands.FindAsync(1);

        context.Brands.Remove(brand!);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    // ---------- Rename ----------

    [Fact]
    public async Task RenameBrand_ChangesName()
    {
        var brand = await _service.AddBrandAsync("OldName");

        var result = await _service.RenameBrandAsync(brand.Id, "NewName");

        Assert.True(result);
        var brands = await _service.GetBrandsAsync();
        Assert.Contains(brands, b => b.Id == brand.Id && b.Name == "NewName");
    }

    [Fact]
    public async Task RenameBrand_DuplicateName_ReturnsFalse()
    {
        var brand = await _service.AddBrandAsync("UniqueName");

        // "Novritsch" bestaat al als seed-merk
        var result = await _service.RenameBrandAsync(brand.Id, "Novritsch");

        Assert.False(result);
    }

    [Fact]
    public async Task RenameBrand_EmptyName_ReturnsFalse()
    {
        var brand = await _service.AddBrandAsync("SomeBrand");

        var result = await _service.RenameBrandAsync(brand.Id, "   ");

        Assert.False(result);
    }
}
