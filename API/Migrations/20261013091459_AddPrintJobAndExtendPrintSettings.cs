using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MasterBilling.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPrintJobAndExtendPrintSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PrintSettings_TenantId",
                table: "PrintSettings");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "PrintSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CounterId",
                table: "PrintSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterText",
                table: "PrintSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OpenCashDrawer",
                table: "PrintSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrinterConnection",
                table: "PrintSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrinterIpAddress",
                table: "PrintSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrinterPort",
                table: "PrintSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PrintJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentType = table.Column<string>(type: "text", nullable: false),
                    DocumentId = table.Column<int>(type: "integer", nullable: true),
                    DocumentReference = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    PrinterTarget = table.Column<string>(type: "text", nullable: true),
                    PaperSize = table.Column<string>(type: "text", nullable: true),
                    Copies = table.Column<int>(type: "integer", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    PrintedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryAfter = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsReprint = table.Column<bool>(type: "boolean", nullable: false),
                    OriginalPrintJobId = table.Column<int>(type: "integer", nullable: true),
                    InitiatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    CounterId = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_PrintJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrintJobs_Businesses_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrintJobs_Users_InitiatedByUserId",
                        column: x => x.InitiatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 9, 14, 56, 997, DateTimeKind.Utc).AddTicks(3598));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 9, 14, 56, 997, DateTimeKind.Utc).AddTicks(3604));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 9, 14, 56, 997, DateTimeKind.Utc).AddTicks(3605));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 9, 14, 56, 997, DateTimeKind.Utc).AddTicks(3606));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 9, 14, 56, 997, DateTimeKind.Utc).AddTicks(3607));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 9, 14, 56, 997, DateTimeKind.Utc).AddTicks(3608));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 9, 14, 56, 997, DateTimeKind.Utc).AddTicks(3612));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 9, 14, 56, 997, DateTimeKind.Utc).AddTicks(3613));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 9, 14, 56, 997, DateTimeKind.Utc).AddTicks(3614));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 9, 14, 56, 997, DateTimeKind.Utc).AddTicks(3615));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 9, 14, 57, 190, DateTimeKind.Utc).AddTicks(946));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 9, 14, 57, 190, DateTimeKind.Utc).AddTicks(950));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 9, 14, 57, 190, DateTimeKind.Utc).AddTicks(952));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 9, 14, 57, 190, DateTimeKind.Utc).AddTicks(954));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 9, 14, 57, 190, DateTimeKind.Utc).AddTicks(955));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Dv6UrOWV2FgLL8x6MAcrtOJFKc7Maul4RhFUrjpF45FY2zQl.5v7K");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$5dYZ54/jlJKzpb0xqAK2cOdIazj0OWZZGZPbGm3DhjjOUOwpXhwqm");

            migrationBuilder.CreateIndex(
                name: "IX_PrintSettings_BranchId",
                table: "PrintSettings",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintSettings_TenantId_BranchId_CounterId",
                table: "PrintSettings",
                columns: new[] { "TenantId", "BranchId", "CounterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrintJobs_InitiatedByUserId",
                table: "PrintJobs",
                column: "InitiatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintJobs_TenantId_DocumentType_DocumentId",
                table: "PrintJobs",
                columns: new[] { "TenantId", "DocumentType", "DocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_PrintJobs_TenantId_Status",
                table: "PrintJobs",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_PrintSettings_Branches_BranchId",
                table: "PrintSettings",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrintSettings_Branches_BranchId",
                table: "PrintSettings");

            migrationBuilder.DropTable(
                name: "PrintJobs");

            migrationBuilder.DropIndex(
                name: "IX_PrintSettings_BranchId",
                table: "PrintSettings");

            migrationBuilder.DropIndex(
                name: "IX_PrintSettings_TenantId_BranchId_CounterId",
                table: "PrintSettings");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "PrintSettings");

            migrationBuilder.DropColumn(
                name: "CounterId",
                table: "PrintSettings");

            migrationBuilder.DropColumn(
                name: "FooterText",
                table: "PrintSettings");

            migrationBuilder.DropColumn(
                name: "OpenCashDrawer",
                table: "PrintSettings");

            migrationBuilder.DropColumn(
                name: "PrinterConnection",
                table: "PrintSettings");

            migrationBuilder.DropColumn(
                name: "PrinterIpAddress",
                table: "PrintSettings");

            migrationBuilder.DropColumn(
                name: "PrinterPort",
                table: "PrintSettings");

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 7, 52, 13, 115, DateTimeKind.Utc).AddTicks(8260));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 7, 52, 13, 115, DateTimeKind.Utc).AddTicks(8266));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 7, 52, 13, 115, DateTimeKind.Utc).AddTicks(8269));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 7, 52, 13, 115, DateTimeKind.Utc).AddTicks(8271));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 7, 52, 13, 115, DateTimeKind.Utc).AddTicks(8273));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 7, 52, 13, 115, DateTimeKind.Utc).AddTicks(8275));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 7, 52, 13, 115, DateTimeKind.Utc).AddTicks(8277));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 7, 52, 13, 115, DateTimeKind.Utc).AddTicks(8279));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 7, 52, 13, 115, DateTimeKind.Utc).AddTicks(8281));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 7, 52, 13, 115, DateTimeKind.Utc).AddTicks(8283));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 7, 52, 13, 274, DateTimeKind.Utc).AddTicks(2095));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 7, 52, 13, 274, DateTimeKind.Utc).AddTicks(2101));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 7, 52, 13, 274, DateTimeKind.Utc).AddTicks(2102));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 7, 52, 13, 274, DateTimeKind.Utc).AddTicks(2103));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 13, 7, 52, 13, 274, DateTimeKind.Utc).AddTicks(2104));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$RLMFfLHpCtWcQ6NywaIEQ.i//uZafD2wvYdhwr7iaRkuRJn3VG9.2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$Y75qrPxbRO2d3VAlUkhYuur0.vaeKXvhhpdZOEgyJ5PJ4SgPH/HaS");

            migrationBuilder.CreateIndex(
                name: "IX_PrintSettings_TenantId",
                table: "PrintSettings",
                column: "TenantId");
        }
    }
}
