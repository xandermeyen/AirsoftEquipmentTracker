using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AirsoftEquipmentTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentItems_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipmentItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Brands",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Novritsch" },
                    { 2, "Tokyo Marui" },
                    { 3, "Invader Gear" },
                    { 4, "Vector Optics" },
                    { 5, "Specna Arms" },
                    { 6, "Titan Power" },
                    { 7, "Cyma" },
                    { 8, "BLS" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Primary" },
                    { 2, "Secondary" },
                    { 3, "Gear" },
                    { 4, "Attachment" },
                    { 5, "Magazine" },
                    { 6, "Consumable" }
                });

            migrationBuilder.InsertData(
                table: "EquipmentItems",
                columns: new[] { "Id", "BrandId", "CategoryId", "Name", "Notes", "Price", "PurchaseDate" },
                values: new object[,]
                {
                    { 1, 1, 1, "SSR90", "Main outdoor replica", 549m, new DateTime(2024, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 2, 2, "Glock 17", "Sidearm", 180m, new DateTime(2023, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, 3, 3, "Plate Carrier", "Main carrier setup", 120m, new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, 4, 4, "Red Dot Sight", "Mounted on SSR90", 90m, new DateTime(2024, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, 5, 5, "Mid-Cap Magazine", "120 round mid-cap", 15m, new DateTime(2024, 3, 2, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 6, 6, 3, "LiPo Battery 11.1V", "Used for SSR90", 35m, new DateTime(2024, 4, 18, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 7, 7, 3, "Speedloader", "For loading magazines", 8m, new DateTime(2023, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 8, 8, 6, "0.28g BB Bottle", "Outdoor BBs", 18m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentItems_BrandId",
                table: "EquipmentItems",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentItems_CategoryId",
                table: "EquipmentItems",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentItems");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
