using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace fintech_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Transactions",
                columns: new[] { "Id", "Amount", "Category", "CreatedAtUtc", "Description", "DestinationAccountId", "SourceAccountId", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("c1111111-1111-1111-1111-111111111111"), 1100.00m, "Bank", new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Initial Deposit to Alice", new Guid("a3333333-3333-3333-3333-333333333333"), null, "Deposit", new Guid("a1111111-1111-1111-1111-111111111111") },
                    { new Guid("c2222222-2222-2222-2222-222222222222"), 400.00m, "Bank", new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Initial Deposit to Bob", new Guid("b4444444-4444-4444-4444-444444444444"), null, "Deposit", new Guid("b2222222-2222-2222-2222-222222222222") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("c2222222-2222-2222-2222-222222222222"));
        }
    }
}
