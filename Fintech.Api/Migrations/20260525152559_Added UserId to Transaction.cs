using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace fintech_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserIdtoTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("a3333333-3333-3333-3333-333333333333"),
                column: "Name",
                value: "Alice Account");

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("b4444444-4444-4444-4444-444444444444"),
                column: "Name",
                value: "Bob Account");

            migrationBuilder.InsertData(
                table: "Transactions",
                columns: new[] { "Id", "Amount", "Category", "CreatedAtUtc", "Description", "DestinationAccountId", "SourceAccountId", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("a5555555-5555-5555-5555-555555555555"), 100.00m, "Dining", new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Transfer to Bob", new Guid("b4444444-4444-4444-4444-444444444444"), new Guid("a3333333-3333-3333-3333-333333333333"), "Transfer", new Guid("a1111111-1111-1111-1111-111111111111") },
                    { new Guid("b6666666-6666-6666-6666-666666666666"), 100.00m, "Mortgage", new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Deposit to Alice", new Guid("a3333333-3333-3333-3333-333333333333"), null, "Deposit", new Guid("a1111111-1111-1111-1111-111111111111") },
                    { new Guid("b7777777-7777-7777-7777-777777777777"), 100.00m, "Gym", new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Withdrawal from Alice", null, new Guid("a3333333-3333-3333-3333-333333333333"), "Withdrawal", new Guid("a1111111-1111-1111-1111-111111111111") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions");

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("a5555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("b6666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("b7777777-7777-7777-7777-777777777777"));

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Transactions");

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("a3333333-3333-3333-3333-333333333333"),
                column: "Name",
                value: "New Account");

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("b4444444-4444-4444-4444-444444444444"),
                column: "Name",
                value: "New Account");
        }
    }
}
