using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MasterBilling.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFullPaymentSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "Payments",
                newName: "CashierName");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Payments",
                newName: "TotalAmount");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "Refunds",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAmount",
                table: "Payments",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ChangeAmount",
                table: "Payments",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "Payments",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ProcessedByUserId",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QRCodeData",
                table: "PaymentGatewayTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCard",
                table: "CashRegisters",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCash",
                table: "CashRegisters",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalOther",
                table: "CashRegisters",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalUPI",
                table: "CashRegisters",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "PaymentConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_PaymentConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentConfigurations_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentConfigurations_Businesses_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentId = table.Column<int>(type: "integer", nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CashReceived = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ChangeGiven = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "text", nullable: true),
                    TransactionId = table.Column<string>(type: "text", nullable: true),
                    UPIApp = table.Column<string>(type: "text", nullable: true),
                    CardLastFour = table.Column<string>(type: "text", nullable: true),
                    CardNetwork = table.Column<string>(type: "text", nullable: true),
                    BankName = table.Column<string>(type: "text", nullable: true),
                    WalletName = table.Column<string>(type: "text", nullable: true),
                    GiftCardCode = table.Column<string>(type: "text", nullable: true),
                    ChequeNumber = table.Column<string>(type: "text", nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EMIBankName = table.Column<string>(type: "text", nullable: true),
                    EMIMonths = table.Column<int>(type: "integer", nullable: true),
                    EMIInterestRate = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PaymentDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentDetails_Businesses_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentDetails_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentProviders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProviderName = table.Column<string>(type: "text", nullable: false),
                    ProviderCode = table.Column<string>(type: "text", nullable: false),
                    WebhookSecret = table.Column<string>(type: "text", nullable: true),
                    ApiEndpoint = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SupportedMethods = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentProviders_Businesses_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTerminals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TerminalName = table.Column<string>(type: "text", nullable: false),
                    TerminalId = table.Column<string>(type: "text", nullable: false),
                    MerchantId = table.Column<string>(type: "text", nullable: true),
                    ProviderId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    ConnectionType = table.Column<string>(type: "text", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_PaymentTerminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTerminals_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentTerminals_Businesses_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentTerminals_PaymentProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "PaymentProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentSettlements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TerminalId = table.Column<int>(type: "integer", nullable: true),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    SettlementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TransactionCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SettlementReference = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PaymentSettlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentSettlements_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentSettlements_Businesses_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentSettlements_PaymentTerminals_TerminalId",
                        column: x => x.TerminalId,
                        principalTable: "PaymentTerminals",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentId = table.Column<int>(type: "integer", nullable: false),
                    TerminalId = table.Column<int>(type: "integer", nullable: true),
                    ProviderId = table.Column<int>(type: "integer", nullable: true),
                    TransactionRef = table.Column<string>(type: "text", nullable: false),
                    ProviderTransactionId = table.Column<string>(type: "text", nullable: true),
                    ProviderOrderId = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusMessage = table.Column<string>(type: "text", nullable: true),
                    RawResponse = table.Column<string>(type: "text", nullable: true),
                    InitiatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Businesses_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_PaymentProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "PaymentProviders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_PaymentTerminals_TerminalId",
                        column: x => x.TerminalId,
                        principalTable: "PaymentTerminals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 18, 16, 22, 18, 633, DateTimeKind.Utc).AddTicks(8867));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 18, 16, 22, 18, 633, DateTimeKind.Utc).AddTicks(8878));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 18, 16, 22, 18, 633, DateTimeKind.Utc).AddTicks(8883));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 18, 16, 22, 18, 633, DateTimeKind.Utc).AddTicks(8887));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 18, 16, 22, 18, 633, DateTimeKind.Utc).AddTicks(8890));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 18, 16, 22, 18, 633, DateTimeKind.Utc).AddTicks(8894));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 18, 16, 22, 18, 633, DateTimeKind.Utc).AddTicks(8897));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 18, 16, 22, 18, 633, DateTimeKind.Utc).AddTicks(8900));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 18, 16, 22, 18, 633, DateTimeKind.Utc).AddTicks(8903));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 18, 16, 22, 18, 633, DateTimeKind.Utc).AddTicks(8906));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 18, 16, 22, 18, 919, DateTimeKind.Utc).AddTicks(9804));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 18, 16, 22, 18, 919, DateTimeKind.Utc).AddTicks(9814));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 18, 16, 22, 18, 919, DateTimeKind.Utc).AddTicks(9819));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 18, 16, 22, 18, 919, DateTimeKind.Utc).AddTicks(9823));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 18, 16, 22, 18, 919, DateTimeKind.Utc).AddTicks(9829));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Pk5lUrkmtcleXXr8qnt94edA8Jz9bGmwG0sx5F7f7KnYAErHHJU5e");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$b2NH1zi0AZxUQe9JqPdtlORMSK3T14kdm6ggAtCwfdof26EIDJ476");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 9999,
                column: "PasswordHash",
                value: "$2a$11$qxeHfTSRCM.69.a3EZ5/KeqQ66fpdnffQ/3w6QehQkgjYTjqvuFQO");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentConfigurations_BranchId",
                table: "PaymentConfigurations",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentConfigurations_TenantId",
                table: "PaymentConfigurations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_PaymentId",
                table: "PaymentDetails",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_TenantId",
                table: "PaymentDetails",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentProviders_TenantId",
                table: "PaymentProviders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSettlements_BranchId",
                table: "PaymentSettlements",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSettlements_TenantId",
                table: "PaymentSettlements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSettlements_TerminalId",
                table: "PaymentSettlements",
                column: "TerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTerminals_BranchId",
                table: "PaymentTerminals",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTerminals_ProviderId",
                table: "PaymentTerminals",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTerminals_TenantId",
                table: "PaymentTerminals",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PaymentId",
                table: "PaymentTransactions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ProviderId",
                table: "PaymentTransactions",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_TenantId",
                table: "PaymentTransactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_TerminalId",
                table: "PaymentTransactions",
                column: "TerminalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentConfigurations");

            migrationBuilder.DropTable(
                name: "PaymentDetails");

            migrationBuilder.DropTable(
                name: "PaymentSettlements");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "PaymentTerminals");

            migrationBuilder.DropTable(
                name: "PaymentProviders");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "BalanceAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ChangeAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ProcessedByUserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "QRCodeData",
                table: "PaymentGatewayTransactions");

            migrationBuilder.DropColumn(
                name: "TotalCard",
                table: "CashRegisters");

            migrationBuilder.DropColumn(
                name: "TotalCash",
                table: "CashRegisters");

            migrationBuilder.DropColumn(
                name: "TotalOther",
                table: "CashRegisters");

            migrationBuilder.DropColumn(
                name: "TotalUPI",
                table: "CashRegisters");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Payments",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "CashierName",
                table: "Payments",
                newName: "TransactionId");

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "Payments",
                type: "text",
                nullable: true);

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
    }
}
