using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasterBilling.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanPermitFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GrantNote",
                table: "Subscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GrantedAt",
                table: "Subscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGrantedBySuperAdmin",
                table: "Subscriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PermitDuration",
                table: "Subscriptions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlanPermit",
                table: "Subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 39, 42, 138, DateTimeKind.Utc).AddTicks(5270));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 39, 42, 138, DateTimeKind.Utc).AddTicks(5274));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 39, 42, 138, DateTimeKind.Utc).AddTicks(5275));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 39, 42, 138, DateTimeKind.Utc).AddTicks(5276));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 39, 42, 138, DateTimeKind.Utc).AddTicks(5277));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 39, 42, 138, DateTimeKind.Utc).AddTicks(5278));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 39, 42, 138, DateTimeKind.Utc).AddTicks(5279));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 39, 42, 138, DateTimeKind.Utc).AddTicks(5279));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 39, 42, 138, DateTimeKind.Utc).AddTicks(5280));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 39, 42, 138, DateTimeKind.Utc).AddTicks(5289));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 39, 42, 302, DateTimeKind.Utc).AddTicks(3560));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 39, 42, 302, DateTimeKind.Utc).AddTicks(3562));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 39, 42, 302, DateTimeKind.Utc).AddTicks(3564));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 39, 42, 302, DateTimeKind.Utc).AddTicks(3565));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 39, 42, 302, DateTimeKind.Utc).AddTicks(3566));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$J9Oxq3zBahBqV.Uy.o2iZugbG.Ed3JilfiIxZLv2.EJ5OtkYWTZZG");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$8p4bJXk1rDKdI5FCeGrQs.WeV8A5DacKbOGRrFDJblQA57hKFc0Hi");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 9999,
                column: "PasswordHash",
                value: "$2a$11$/bycItNE2DQeSBLq6/AF0OkCH/fN48LfUhz8TfNnYxRklHZe1Dv5e");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GrantNote",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "GrantedAt",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "IsGrantedBySuperAdmin",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PermitDuration",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PlanPermit",
                table: "Subscriptions");

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 0, 32, 449, DateTimeKind.Utc).AddTicks(951));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 0, 32, 449, DateTimeKind.Utc).AddTicks(955));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 0, 32, 449, DateTimeKind.Utc).AddTicks(956));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 0, 32, 449, DateTimeKind.Utc).AddTicks(957));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 0, 32, 449, DateTimeKind.Utc).AddTicks(958));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 0, 32, 449, DateTimeKind.Utc).AddTicks(959));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 0, 32, 449, DateTimeKind.Utc).AddTicks(960));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 0, 32, 449, DateTimeKind.Utc).AddTicks(961));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 0, 32, 449, DateTimeKind.Utc).AddTicks(961));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 0, 32, 449, DateTimeKind.Utc).AddTicks(962));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 0, 32, 558, DateTimeKind.Utc).AddTicks(1927));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 0, 32, 558, DateTimeKind.Utc).AddTicks(1932));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 0, 32, 558, DateTimeKind.Utc).AddTicks(1933));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 0, 32, 558, DateTimeKind.Utc).AddTicks(1934));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 1, 12, 0, 32, 558, DateTimeKind.Utc).AddTicks(1935));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$I3GFGgHSaOTd73Uex.gyTuhY2hvWvAr3bczyJ7htG4GjgzGtdBvF.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$xfTuVBIa5Ymc6rpjm84DOOUbCe2cP6itK1hAHWpdToffahr4p6LEO");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 9999,
                column: "PasswordHash",
                value: "$2a$11$mZm2qb0ASEZcNq.xVUm0e.d5D.owFzOM.7w5XgSDsMyLkEjvONppy");
        }
    }
}
