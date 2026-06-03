using Airsoft_equipment_tracker.Models;
using Microsoft.EntityFrameworkCore;

namespace Airsoft_equipment_tracker.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<EquipmentItem> EquipmentItems => Set<EquipmentItem>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Category> Categories => Set<Category>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EquipmentItem>()
                .Property(e => e.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<EquipmentItem>()
                .HasOne(e => e.Brand)
                .WithMany()
                .HasForeignKey(e => e.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EquipmentItem>()
                .HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Primary" },
                new Category { Id = 2, Name = "Secondary" },
                new Category { Id = 3, Name = "Gear" },
                new Category { Id = 4, Name = "Attachment" },
                new Category { Id = 5, Name = "Magazine" },
                new Category { Id = 6, Name = "Consumable" }
            );

            modelBuilder.Entity<Brand>().HasData(
                new Brand { Id = 1, Name = "Novritsch" },
                new Brand { Id = 2, Name = "Tokyo Marui" },
                new Brand { Id = 3, Name = "Invader Gear" },
                new Brand { Id = 4, Name = "Vector Optics" },
                new Brand { Id = 5, Name = "Specna Arms" },
                new Brand { Id = 6, Name = "Titan Power" },
                new Brand { Id = 7, Name = "Cyma" },
                new Brand { Id = 8, Name = "BLS" }
            );

            modelBuilder.Entity<EquipmentItem>().HasData(
                new EquipmentItem { Id = 1, Name = "SSR90", Price = 549m, PurchaseDate = new DateTime(2024, 5, 10), BrandId = 1, CategoryId = 1, Notes = "Main outdoor replica" },
                new EquipmentItem { Id = 2, Name = "Glock 17", Price = 180m, PurchaseDate = new DateTime(2023, 8, 12), BrandId = 2, CategoryId = 2, Notes = "Sidearm" },
                new EquipmentItem { Id = 3, Name = "Plate Carrier", Price = 120m, PurchaseDate = new DateTime(2024, 1, 20), BrandId = 3, CategoryId = 3, Notes = "Main carrier setup" },
                new EquipmentItem { Id = 4, Name = "Red Dot Sight", Price = 90m, PurchaseDate = new DateTime(2024, 2, 14), BrandId = 4, CategoryId = 4, Notes = "Mounted on SSR90" },
                new EquipmentItem { Id = 5, Name = "Mid-Cap Magazine", Price = 15m, PurchaseDate = new DateTime(2024, 3, 2), BrandId = 5, CategoryId = 5, Notes = "120 round mid-cap" },
                new EquipmentItem { Id = 6, Name = "LiPo Battery 11.1V", Price = 35m, PurchaseDate = new DateTime(2024, 4, 18), BrandId = 6, CategoryId = 3, Notes = "Used for SSR90" },
                new EquipmentItem { Id = 7, Name = "Speedloader", Price = 8m, PurchaseDate = new DateTime(2023, 11, 5), BrandId = 7, CategoryId = 3, Notes = "For loading magazines" },
                new EquipmentItem { Id = 8, Name = "0.28g BB Bottle", Price = 18m, PurchaseDate = new DateTime(2024, 6, 1), BrandId = 8, CategoryId = 6, Notes = "Outdoor BBs" }
            );
        }
    }
}