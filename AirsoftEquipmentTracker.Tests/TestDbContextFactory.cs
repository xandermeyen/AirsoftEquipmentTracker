using AirsoftEquipmentTracker.Data;
using AirsoftEquipmentTracker.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AirsoftEquipmentTracker.Tests;

// Factory voor tests: gebruikt een SQLite in-memory database in plaats van SQL Server.
// De connectie moet open blijven zolang de database nodig is, anders verdwijnt
// de in-memory database. Elke testklasse-instantie krijgt zo een verse database,
// inclusief de seed-data uit AppDbContext (8 merken, 6 categorieen).
//
// Er wordt ook een test-gebruiker aangemaakt zodat de FK-constraint op
// EquipmentItem.UserId niet faalt in tests.
public class TestDbContextFactory : IDbContextFactory<AppDbContext>, IDisposable
{
    public const string TestUserId  = "test-user-id";
    public const string OtherUserId = "other-user-id";

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

        // Seed twee gebruikers zodat EquipmentItem.UserId FK-checks slagen
        context.Users.AddRange(
            new ApplicationUser
            {
                Id            = TestUserId,
                UserName      = "test@example.com",
                NormalizedUserName = "TEST@EXAMPLE.COM",
                Email         = "test@example.com",
                NormalizedEmail = "TEST@EXAMPLE.COM",
                SecurityStamp = Guid.NewGuid().ToString()
            },
            new ApplicationUser
            {
                Id            = OtherUserId,
                UserName      = "other@example.com",
                NormalizedUserName = "OTHER@EXAMPLE.COM",
                Email         = "other@example.com",
                NormalizedEmail = "OTHER@EXAMPLE.COM",
                SecurityStamp = Guid.NewGuid().ToString()
            }
        );
        context.SaveChanges();
    }

    public AppDbContext CreateDbContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}
