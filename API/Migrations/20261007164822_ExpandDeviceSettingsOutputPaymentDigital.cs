using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingSoftware.API.Migrations
{
    /// <inheritdoc />
    public partial class ExpandDeviceSettingsOutputPaymentDigital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "A4ColorMode",
                table: "DeviceSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "A4PaperSize",
                table: "DeviceSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "A4PrinterCopies",
                table: "DeviceSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "A4PrinterEnabled",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "A4PrinterIsDefault",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "A4PrinterName",
                table: "DeviceSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CashDrawerConnectionType",
                table: "DeviceSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CashDrawerTrigger",
                table: "DeviceSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CustomerDisplayProtocol",
                table: "DeviceSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "CustomerDisplayShowShopName",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomerDisplayWelcomeMessage",
                table: "DeviceSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailAutoSendOnSale",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailDeliveryEnabled",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LabelHeightMm",
                table: "DeviceSettings",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LabelPerRow",
                table: "DeviceSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "LabelPrinterEnabled",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LabelPrinterName",
                table: "DeviceSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LabelShowBarcode",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LabelShowBusiness",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LabelShowMRP",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LabelWidthMm",
                table: "DeviceSettings",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PDFAutoGenerate",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PDFEnabled",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PDFSaveToStorage",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentGatewayApiKey",
                table: "DeviceSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentGatewayApiSecret",
                table: "DeviceSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PaymentGatewayEnabled",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentGatewayProvider",
                table: "DeviceSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentGatewayWebhookUrl",
                table: "DeviceSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SMSAutoSendOnSale",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SMSDeliveryEnabled",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SpeakerAnnouncePayment",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SpeakerAnnounceTotal",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SpeakerBeepOnScan",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SpeakerEnabled",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ThermalAutoPrint",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ThermalOpenCashDrawer",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ThermalPaperSize",
                table: "DeviceSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ThermalPrinterConnection",
                table: "DeviceSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ThermalPrinterCopies",
                table: "DeviceSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ThermalPrinterEnabled",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ThermalPrinterName",
                table: "DeviceSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThermalPrinterPort",
                table: "DeviceSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UPIEnabled",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UPIId",
                table: "DeviceSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UPIMerchantName",
                table: "DeviceSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UPIShowOnCustomerDisplay",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UPIShowOnReceipt",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatsAppAutoSendOnSale",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatsAppDeliveryEnabled",
                table: "DeviceSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DeviceCategory",
                table: "DeviceConfigurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "DeviceConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "DeviceConfigurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NetworkPort",
                table: "DeviceConfigurations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaperSize",
                table: "DeviceConfigurations",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 7, 16, 48, 19, 704, DateTimeKind.Utc).AddTicks(6789));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 7, 16, 48, 19, 704, DateTimeKind.Utc).AddTicks(6799));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 7, 16, 48, 19, 704, DateTimeKind.Utc).AddTicks(6800));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 7, 16, 48, 19, 704, DateTimeKind.Utc).AddTicks(6801));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 7, 16, 48, 19, 704, DateTimeKind.Utc).AddTicks(6802));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 7, 16, 48, 19, 704, DateTimeKind.Utc).AddTicks(6803));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 7, 16, 48, 19, 704, DateTimeKind.Utc).AddTicks(6804));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 7, 16, 48, 19, 704, DateTimeKind.Utc).AddTicks(6805));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 7, 16, 48, 19, 704, DateTimeKind.Utc).AddTicks(6806));

            migrationBuilder.UpdateData(
                table: "BusinessTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 7, 16, 48, 19, 704, DateTimeKind.Utc).AddTicks(6807));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 7, 16, 48, 19, 885, DateTimeKind.Utc).AddTicks(7044));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 7, 16, 48, 19, 885, DateTimeKind.Utc).AddTicks(7049));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 7, 16, 48, 19, 885, DateTimeKind.Utc).AddTicks(7050));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 7, 16, 48, 19, 885, DateTimeKind.Utc).AddTicks(7051));

            migrationBuilder.UpdateData(
                table: "Lookups",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 7, 16, 48, 19, 885, DateTimeKind.Utc).AddTicks(7052));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$dyXPvvhVB7c2pJ76h5Omvumu5pJrmlbik.uvTB/un76U3r.n/eClW");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "A4ColorMode",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "A4PaperSize",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "A4PrinterCopies",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "A4PrinterEnabled",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "A4PrinterIsDefault",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "A4PrinterName",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "CashDrawerConnectionType",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "CashDrawerTrigger",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "CustomerDisplayProtocol",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "CustomerDisplayShowShopName",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "CustomerDisplayWelcomeMessage",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "EmailAutoSendOnSale",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "EmailDeliveryEnabled",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "LabelHeightMm",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "LabelPerRow",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "LabelPrinterEnabled",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "LabelPrinterName",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "LabelShowBarcode",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "LabelShowBusiness",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "LabelShowMRP",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "LabelWidthMm",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "PDFAutoGenerate",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "PDFEnabled",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "PDFSaveToStorage",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "PaymentGatewayApiKey",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "PaymentGatewayApiSecret",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "PaymentGatewayEnabled",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "PaymentGatewayProvider",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "PaymentGatewayWebhookUrl",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "SMSAutoSendOnSale",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "SMSDeliveryEnabled",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "SpeakerAnnouncePayment",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "SpeakerAnnounceTotal",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "SpeakerBeepOnScan",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "SpeakerEnabled",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "ThermalAutoPrint",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "ThermalOpenCashDrawer",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "ThermalPaperSize",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "ThermalPrinterConnection",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "ThermalPrinterCopies",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "ThermalPrinterEnabled",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "ThermalPrinterName",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "ThermalPrinterPort",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "UPIEnabled",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "UPIId",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "UPIMerchantName",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "UPIShowOnCustomerDisplay",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "UPIShowOnReceipt",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "WhatsAppAutoSendOnSale",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "WhatsAppDeliveryEnabled",
                table: "DeviceSettings");

            migrationBuilder.DropColumn(
                name: "DeviceCategory",
                table: "DeviceConfigurations");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "DeviceConfigurations");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "DeviceConfigurations");

            migrationBuilder.DropColumn(
                name: "NetworkPort",
                table: "DeviceConfigurations");

            migrationBuilder.DropColumn(
                name: "PaperSize",
                table: "DeviceConfigurations");

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
        }
    }
}
