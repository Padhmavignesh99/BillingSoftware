using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingSoftware.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsIntegrationToSubscriptionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasSmsIntegration",
                table: "SubscriptionPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 16, 43, 5, 26, DateTimeKind.Utc).AddTicks(2165));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 16, 43, 5, 26, DateTimeKind.Utc).AddTicks(2175));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 16, 43, 5, 26, DateTimeKind.Utc).AddTicks(2180));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 16, 43, 5, 26, DateTimeKind.Utc).AddTicks(2184));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 16, 43, 5, 26, DateTimeKind.Utc).AddTicks(2188));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 16, 43, 5, 26, DateTimeKind.Utc).AddTicks(2191));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 16, 43, 5, 26, DateTimeKind.Utc).AddTicks(2195));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 16, 43, 5, 26, DateTimeKind.Utc).AddTicks(2198));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 16, 43, 5, 26, DateTimeKind.Utc).AddTicks(2202));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 16, 43, 5, 26, DateTimeKind.Utc).AddTicks(2206));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 16, 43, 5, 428, DateTimeKind.Utc).AddTicks(6889));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 16, 43, 5, 428, DateTimeKind.Utc).AddTicks(6899));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 16, 43, 5, 428, DateTimeKind.Utc).AddTicks(6907));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 16, 43, 5, 428, DateTimeKind.Utc).AddTicks(6912));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 16, 43, 5, 428, DateTimeKind.Utc).AddTicks(6916));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "HasSmsIntegration",
                value: false);

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "HasSmsIntegration",
                value: false);

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "HasSmsIntegration",
                value: false);

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 4,
                column: "HasSmsIntegration",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$XohkG/B.5Q//ss2Dn3EzOuh61XLJqwrQdNQjQdBxFzyFVxAwLGcdq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$qtq8oS0rN6kHzUU1gGB6mObHJBaxSFhYVyWwtBYkrXqhDC7wOwcjC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasSmsIntegration",
                table: "SubscriptionPlans");

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 16, 58, 35, 644, DateTimeKind.Utc).AddTicks(1614));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 16, 58, 35, 644, DateTimeKind.Utc).AddTicks(1617));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 16, 58, 35, 644, DateTimeKind.Utc).AddTicks(1618));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 16, 58, 35, 644, DateTimeKind.Utc).AddTicks(1619));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 16, 58, 35, 644, DateTimeKind.Utc).AddTicks(1620));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 16, 58, 35, 644, DateTimeKind.Utc).AddTicks(1621));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 16, 58, 35, 644, DateTimeKind.Utc).AddTicks(1622));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 16, 58, 35, 644, DateTimeKind.Utc).AddTicks(1623));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 16, 58, 35, 644, DateTimeKind.Utc).AddTicks(1623));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 16, 58, 35, 644, DateTimeKind.Utc).AddTicks(1624));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 16, 58, 35, 775, DateTimeKind.Utc).AddTicks(255));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 16, 58, 35, 775, DateTimeKind.Utc).AddTicks(259));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 16, 58, 35, 775, DateTimeKind.Utc).AddTicks(261));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 16, 58, 35, 775, DateTimeKind.Utc).AddTicks(262));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 16, 58, 35, 775, DateTimeKind.Utc).AddTicks(263));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$m3iP7tjxbKn1Wq4DmuJZgOr3rVzT5A4TQ.GEpNhjqVtbFl22l1cPy");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$ZrETxNTsxeD9pZKiBIX/Uu9wQLv5YMFmvmznGE3uPm4Mva5GxKiU6");
        }
    }
}
