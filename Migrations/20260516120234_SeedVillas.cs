using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RoyalVillaApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedVillas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                table: "Villas",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.InsertData(
                table: "Villas",
                columns: new[] { "Id", "CreatedDate", "Details", "ImageUrl", "Name", "Occupancy", "Rate", "Sqft", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Luxurious villa with stunning ocean views and private beach access.", "https://dotnetmasteryimages.blob.core.windows.net/bluevillaimages/villa1.jpg", "Royal Villa", 6, 500.0, 2500, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Elegant villa with marble interiors and panoramic mountain views.", "https://dotnetmasteryimages.blob.core.windows.net/bluevillaimages/villa2.jpg", "Diamond Villa", 8, 750.0, 3200, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Modern villa featuring an infinity pool and outdoor entertainment area.", "https://dotnetmasteryimages.blob.core.windows.net/bluevillaimages/villa3.jpg", "Pool Villa", 4, 350.0, 1800, new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2024, 2, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Premium villa with spa facilities and concierge services.", "https://dotnetmasteryimages.blob.core.windows.net/bluevillaimages/villa4.jpg", "Luxury Villa", 10, 900.0, 4000, new DateTime(2024, 2, 14, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Charming villa surrounded by tropical gardens and nature trails.", "https://dotnetmasteryimages.blob.core.windows.net/bluevillaimages/villa5.jpg", "Garden Villa", 3, 275.0, 1500, new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                table: "Villas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
