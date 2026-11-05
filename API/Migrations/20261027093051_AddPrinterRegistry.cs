using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MasterBilling.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPrinterRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Printers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrinterName = table.Column<string>(type: "text", nullable: false),
                    PrinterType = table.Column<string>(type: "text", nullable: false),
                    ConnectionType = table.Column<string>(type: "text", nullable: false),
                    ConnectionAddress = table.Column<string>(type: "text", nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: true),
                    OsDriverName = table.Column<string>(type: "text", nullable: true),
                    PaperSize = table.Column<string>(type: "text", nullable: false),
                    Orientation = table.Column<string>(type: "text", nullable: false),
                    CharsPerLine = table.Column<int>(type: "integer", nullable: true),
                    DpiResolution = table.Column<int>(type: "integer", nullable: true),
                    SupportsColor = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsAutoCut = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsCashDrawer = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsBarcodes = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsQRCode = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsLogo = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsEscPos = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsPdf = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    CounterId = table.Column<int>(type: "integer", nullable: true),
                    CodePage = table.Column<int>(type: "integer", nullable: true),
                    LineFeedCount = table.Column<int>(type: "integer", nullable: false),
                    LastTestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastTestStatus = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Printers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Printers_Businesses_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 27, 9, 30, 46, 237, DateTimeKind.Utc).AddTicks(230));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 27, 9, 30, 46, 237, DateTimeKind.Utc).AddTicks(234));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 27, 9, 30, 46, 237, DateTimeKind.Utc).AddTicks(236));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 27, 9, 30, 46, 237, DateTimeKind.Utc).AddTicks(237));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 27, 9, 30, 46, 237, DateTimeKind.Utc).AddTicks(238));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 27, 9, 30, 46, 237, DateTimeKind.Utc).AddTicks(239));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 27, 9, 30, 46, 237, DateTimeKind.Utc).AddTicks(240));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 27, 9, 30, 46, 237, DateTimeKind.Utc).AddTicks(241));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 27, 9, 30, 46, 237, DateTimeKind.Utc).AddTicks(242));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 27, 9, 30, 46, 237, DateTimeKind.Utc).AddTicks(243));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 27, 9, 30, 46, 424, DateTimeKind.Utc).AddTicks(7660));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 27, 9, 30, 46, 424, DateTimeKind.Utc).AddTicks(7664));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 27, 9, 30, 46, 424, DateTimeKind.Utc).AddTicks(7666));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 27, 9, 30, 46, 424, DateTimeKind.Utc).AddTicks(7779));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 27, 9, 30, 46, 424, DateTimeKind.Utc).AddTicks(7781));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$05W1F29NgHfQGlVfC7Vld.haW0NhwFk0Xaa9.6zW/.CJxy0gckhEm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$AejEM1corgPQOu7Vp3yHoerPDDvwCVoZURw/qH0f/yQxTNHnWiOB.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 9999,
                column: "PasswordHash",
                value: "$2a$11$W5uBpRk28gT.2aYkQ3cZdOtqU8CSj5Fwe3Pc8DgGfEbp8N52dwHDa");

            migrationBuilder.CreateIndex(
                name: "IX_Printers_TenantId_BranchId_CounterId",
                table: "Printers",
                columns: new[] { "TenantId", "BranchId", "CounterId" });

            migrationBuilder.CreateIndex(
                name: "IX_Printers_TenantId_PrinterType",
                table: "Printers",
                columns: new[] { "TenantId", "PrinterType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Printers");

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 20, 14, 48, 36, 75, DateTimeKind.Utc).AddTicks(6286));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 20, 14, 48, 36, 75, DateTimeKind.Utc).AddTicks(6294));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 20, 14, 48, 36, 75, DateTimeKind.Utc).AddTicks(6298));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 20, 14, 48, 36, 75, DateTimeKind.Utc).AddTicks(6301));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 20, 14, 48, 36, 75, DateTimeKind.Utc).AddTicks(6304));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 20, 14, 48, 36, 75, DateTimeKind.Utc).AddTicks(6307));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 20, 14, 48, 36, 75, DateTimeKind.Utc).AddTicks(6310));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 20, 14, 48, 36, 75, DateTimeKind.Utc).AddTicks(6312));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 20, 14, 48, 36, 75, DateTimeKind.Utc).AddTicks(6315));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 20, 14, 48, 36, 75, DateTimeKind.Utc).AddTicks(6317));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 20, 14, 48, 36, 463, DateTimeKind.Utc).AddTicks(4271));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 20, 14, 48, 36, 463, DateTimeKind.Utc).AddTicks(4279));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 20, 14, 48, 36, 463, DateTimeKind.Utc).AddTicks(4283));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 20, 14, 48, 36, 463, DateTimeKind.Utc).AddTicks(4287));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 20, 14, 48, 36, 463, DateTimeKind.Utc).AddTicks(4289));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$9.UACWzIkesu27h7pPgRDuj1V0csPa7DYKwcz7Vuk/FB5hO7b6N4C");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$4vT.297O/fjjaFtCds7MrO7M2bj7TlK7iiQrqbf97cNzyaWnKrmd.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 9999,
                column: "PasswordHash",
                value: "$2a$11$pe18Ue/QuLI2LZOlnckwmuJGqYg9cbqAm1YxY3oH.4Ai2wNsWMntq");
        }
    }
}
