using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MasterBilling.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceIntegrationLayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardPaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceId = table.Column<int>(type: "integer", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovalCode = table.Column<string>(type: "text", nullable: true),
                    ProviderTransactionId = table.Column<string>(type: "text", nullable: true),
                    CardType = table.Column<string>(type: "text", nullable: true),
                    CardLast4 = table.Column<string>(type: "text", nullable: true),
                    TerminalProvider = table.Column<string>(type: "text", nullable: true),
                    TerminalId = table.Column<string>(type: "text", nullable: true),
                    RawResponseJson = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    InitiatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_CardPaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardPaymentTransactions_Businesses_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardPaymentTransactions_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CardPaymentTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DeviceType = table.Column<int>(type: "integer", nullable: false),
                    ConnectionType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PortOrAddress = table.Column<string>(type: "text", nullable: true),
                    BaudRate = table.Column<int>(type: "integer", nullable: true),
                    Model = table.Column<string>(type: "text", nullable: true),
                    SerialNumber = table.Column<string>(type: "text", nullable: true),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    ConfigurationJson = table.Column<string>(type: "text", nullable: true),
                    LastConnectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_DeviceConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceConfigurations_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceConfigurations_Businesses_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BarcodeScannerEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    BarcodeScannerSuffix = table.Column<string>(type: "text", nullable: false),
                    BarcodeScannerMinLength = table.Column<int>(type: "integer", nullable: false),
                    BarcodeScannerThresholdMs = table.Column<int>(type: "integer", nullable: false),
                    QRScannerEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CameraQREnabled = table.Column<bool>(type: "boolean", nullable: false),
                    WeighingScaleEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    WeighingScaleMode = table.Column<int>(type: "integer", nullable: false),
                    WeighingScaleUnit = table.Column<int>(type: "integer", nullable: false),
                    WeighingScaleDecimalPlaces = table.Column<int>(type: "integer", nullable: false),
                    WeighingScalePort = table.Column<string>(type: "text", nullable: true),
                    WeighingScaleBaudRate = table.Column<int>(type: "integer", nullable: false),
                    CardTerminalEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CardTerminalProvider = table.Column<string>(type: "text", nullable: true),
                    CardTerminalApiKey = table.Column<string>(type: "text", nullable: true),
                    CardTerminalId = table.Column<string>(type: "text", nullable: true),
                    CashDrawerEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CashDrawerPort = table.Column<string>(type: "text", nullable: true),
                    CustomerDisplayEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CustomerDisplayPort = table.Column<string>(type: "text", nullable: true),
                    OcrScannerEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    OcrProvider = table.Column<string>(type: "text", nullable: true),
                    OcrApiKey = table.Column<string>(type: "text", nullable: true),
                    TouchscreenModeEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RFIDEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    BiometricEnabled = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_DeviceSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceSettings_Businesses_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OcrScanResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentFileUrl = table.Column<string>(type: "text", nullable: true),
                    ExtractedText = table.Column<string>(type: "text", nullable: true),
                    ParsedDataJson = table.Column<string>(type: "text", nullable: true),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    LinkedPurchaseId = table.Column<int>(type: "integer", nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    OcrProvider = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_OcrScanResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OcrScanResults_Businesses_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OcrScanResults_Purchases_LinkedPurchaseId",
                        column: x => x.LinkedPurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OcrScanResults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QRScanEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QRContent = table.Column<string>(type: "text", nullable: false),
                    Purpose = table.Column<int>(type: "integer", nullable: false),
                    Module = table.Column<int>(type: "integer", nullable: false),
                    ResolvedProductId = table.Column<int>(type: "integer", nullable: true),
                    UPIString = table.Column<string>(type: "text", nullable: true),
                    UPIAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsFromCamera = table.Column<bool>(type: "boolean", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    UnresolvedReason = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_QRScanEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QRScanEvents_Businesses_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QRScanEvents_Products_ResolvedProductId",
                        column: x => x.ResolvedProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QRScanEvents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WeightCaptureLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Weight = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    WeightUnit = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    Module = table.Column<int>(type: "integer", nullable: false),
                    CaptureMode = table.Column<int>(type: "integer", nullable: false),
                    RatePerUnit = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ComputedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_WeightCaptureLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeightCaptureLogs_Businesses_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeightCaptureLogs_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WeightCaptureLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScanEventLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScannedValue = table.Column<string>(type: "text", nullable: false),
                    Module = table.Column<int>(type: "integer", nullable: false),
                    ResolvedProductId = table.Column<int>(type: "integer", nullable: true),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    UnresolvedReason = table.Column<string>(type: "text", nullable: true),
                    DeviceConfigurationId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_ScanEventLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScanEventLogs_Businesses_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScanEventLogs_DeviceConfigurations_DeviceConfigurationId",
                        column: x => x.DeviceConfigurationId,
                        principalTable: "DeviceConfigurations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScanEventLogs_Products_ResolvedProductId",
                        column: x => x.ResolvedProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScanEventLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 6, 7, 45, 27, 502, DateTimeKind.Utc).AddTicks(9246));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 6, 7, 45, 27, 502, DateTimeKind.Utc).AddTicks(9249));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 6, 7, 45, 27, 502, DateTimeKind.Utc).AddTicks(9251));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 6, 7, 45, 27, 502, DateTimeKind.Utc).AddTicks(9252));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 6, 7, 45, 27, 502, DateTimeKind.Utc).AddTicks(9253));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 6, 7, 45, 27, 502, DateTimeKind.Utc).AddTicks(9253));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 6, 7, 45, 27, 502, DateTimeKind.Utc).AddTicks(9254));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 6, 7, 45, 27, 502, DateTimeKind.Utc).AddTicks(9255));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 6, 7, 45, 27, 502, DateTimeKind.Utc).AddTicks(9256));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 6, 7, 45, 27, 502, DateTimeKind.Utc).AddTicks(9257));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 6, 7, 45, 27, 687, DateTimeKind.Utc).AddTicks(4051));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 6, 7, 45, 27, 687, DateTimeKind.Utc).AddTicks(4056));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 6, 7, 45, 27, 687, DateTimeKind.Utc).AddTicks(4057));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 6, 7, 45, 27, 687, DateTimeKind.Utc).AddTicks(4059));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 6, 7, 45, 27, 687, DateTimeKind.Utc).AddTicks(4060));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$wjn0n0fwmr3od/slU9gan.LE2nKBwBwDYR4nFKgeDPlPEdKSVc67W");

            migrationBuilder.CreateIndex(
                name: "IX_CardPaymentTransactions_InvoiceId",
                table: "CardPaymentTransactions",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CardPaymentTransactions_TenantId",
                table: "CardPaymentTransactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CardPaymentTransactions_UserId",
                table: "CardPaymentTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceConfigurations_BranchId",
                table: "DeviceConfigurations",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceConfigurations_TenantId",
                table: "DeviceConfigurations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSettings_TenantId",
                table: "DeviceSettings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OcrScanResults_LinkedPurchaseId",
                table: "OcrScanResults",
                column: "LinkedPurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_OcrScanResults_TenantId",
                table: "OcrScanResults",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OcrScanResults_UserId",
                table: "OcrScanResults",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QRScanEvents_ResolvedProductId",
                table: "QRScanEvents",
                column: "ResolvedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_QRScanEvents_TenantId",
                table: "QRScanEvents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_QRScanEvents_UserId",
                table: "QRScanEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanEventLogs_DeviceConfigurationId",
                table: "ScanEventLogs",
                column: "DeviceConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanEventLogs_ResolvedProductId",
                table: "ScanEventLogs",
                column: "ResolvedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanEventLogs_TenantId",
                table: "ScanEventLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanEventLogs_UserId",
                table: "ScanEventLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WeightCaptureLogs_ProductId",
                table: "WeightCaptureLogs",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WeightCaptureLogs_TenantId",
                table: "WeightCaptureLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_WeightCaptureLogs_UserId",
                table: "WeightCaptureLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardPaymentTransactions");

            migrationBuilder.DropTable(
                name: "DeviceSettings");

            migrationBuilder.DropTable(
                name: "OcrScanResults");

            migrationBuilder.DropTable(
                name: "QRScanEvents");

            migrationBuilder.DropTable(
                name: "ScanEventLogs");

            migrationBuilder.DropTable(
                name: "WeightCaptureLogs");

            migrationBuilder.DropTable(
                name: "DeviceConfigurations");

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 13, 23, 494, DateTimeKind.Utc).AddTicks(7226));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 13, 23, 494, DateTimeKind.Utc).AddTicks(7237));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 13, 23, 494, DateTimeKind.Utc).AddTicks(7241));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 13, 23, 494, DateTimeKind.Utc).AddTicks(7245));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 13, 23, 494, DateTimeKind.Utc).AddTicks(7248));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 13, 23, 494, DateTimeKind.Utc).AddTicks(7251));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 13, 23, 494, DateTimeKind.Utc).AddTicks(7254));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 13, 23, 494, DateTimeKind.Utc).AddTicks(7257));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 13, 23, 494, DateTimeKind.Utc).AddTicks(7259));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 13, 23, 494, DateTimeKind.Utc).AddTicks(7262));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 13, 24, 211, DateTimeKind.Utc).AddTicks(4817));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 13, 24, 211, DateTimeKind.Utc).AddTicks(4828));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 13, 24, 211, DateTimeKind.Utc).AddTicks(4832));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 13, 24, 211, DateTimeKind.Utc).AddTicks(4836));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 13, 24, 211, DateTimeKind.Utc).AddTicks(4839));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$6Sv158KiHEt5mkJ4ZxRG8.vlzPi03oa92JuxMvfBr7mgu6zNwI2Fq");
        }
    }
}
