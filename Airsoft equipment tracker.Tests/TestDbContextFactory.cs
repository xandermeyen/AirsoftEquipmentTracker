using Airsoft_equipment_tracker.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Airsoft_equipment_tracker.Tests;

// Factory voor tests: gebruikt een SQLite in-memory database in plaats van SQL Server.
// De connectie moet open blijven zolang de database nodig is, anders verdwijnt
// de in-memory database. Elke testklasse-instantie krijgt zo een verse database,
// inclusief de seed-data uit AppDbContext (8 items, 8 merken, 6 categorieen).
public class TestDbContextFactory : IDbContextFactory<AppDbContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public TestDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Schema + seed-data aanmaken
        using var context = new AppDbContext(_options);
        context.Database.EnsureCreated();
    }

    public AppDbContext CreateDbContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}
