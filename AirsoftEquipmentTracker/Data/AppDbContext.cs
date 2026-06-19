using AirsoftEquipmentTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AirsoftEquipmentTracker.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<EquipmentItem> EquipmentItems => Set<EquipmentItem>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Category> Categories => Set<Category>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // required for Identity tables

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

            modelBuilder.Entity<EquipmentItem>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Brand>()
                .HasIndex(b => b.Name)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

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

            // Equipment seed data removed — items are now user-specific.
            // Each user adds their own gear after registering.
        }
    }
}
