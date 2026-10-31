using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingSoftware.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoBusinessAndUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.InsertData(
                table: "Businesses",
                columns: new[] { "Id", "Address", "BusinessTypeId", "City", "Country", "CreatedAt", "CreatedBy", "CurrencyCode", "CurrencySymbol", "Email", "GSTNumber", "IsActive", "IsDeleted", "LegalName", "LogoUrl", "Name", "PANNumber", "Phone", "PinCode", "State", "Status", "SuspendedAt", "SuspendedReason", "UpdatedAt", "UpdatedBy", "Website" },
                values: new object[] { 9999, null, null, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "INR", "₹", "padhmavignesh99@gmail.com", null, true, false, null, null, "Vignesh Tex", null, null, null, null, 1, null, null, null, null, null });

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
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "HasSmsIntegration",
                value: true);

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 4,
                column: "HasSmsIntegration",
                value: true);

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

            migrationBuilder.InsertData(
                table: "BusinessSettings",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DateFormat", "EnableBarcode", "EnableEmailNotifications", "EnableGST", "EnableLowStockAlert", "EnableMultiBranch", "EnableSmsNotifications", "EnableWhatsAppNotifications", "IsActive", "IsDeleted", "Language", "LowStockThreshold", "TenantId", "TimeZone", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 9999, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "dd-MM-yyyy", true, true, true, true, false, false, false, true, false, "en", 10, 9999, "Asia/Kolkata", null, null });

            migrationBuilder.InsertData(
                table: "InvoiceSettings",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "FooterNote", "InvoiceNumberFormat", "InvoicePrefix", "IsActive", "IsDeleted", "NextInvoiceNumber", "PrimaryColor", "ShowDiscount", "ShowGST", "ShowLogo", "ShowSignature", "SignatureImageUrl", "TemplateName", "TenantId", "TermsAndConditions", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 9999, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "{PREFIX}-{YEAR}-{NUMBER:5}", "INV", true, false, 1, "#1a73e8", true, true, true, false, null, "Default", 9999, null, null, null });

            migrationBuilder.InsertData(
                table: "Trials",
                columns: new[] { "Id", "BusinessId", "ConvertedSubscriptionId", "CreatedAt", "CreatedBy", "EndDate", "IsActive", "IsConverted", "IsDeleted", "StartDate", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 9999, 9999, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2024, 1, 31, 0, 0, 0, 0, DateTimeKind.Utc), true, false, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Email", "FullName", "IsActive", "IsDeleted", "LastLoginAt", "OTP", "OTPExpiry", "PasswordHash", "Phone", "ProfileImageUrl", "RefreshToken", "RefreshTokenExpiry", "ResetToken", "ResetTokenExpiry", "Role", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 9999, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "padhmavignesh99@gmail.com", "Vignesh", true, false, null, null, null, "$2a$11$pe18Ue/QuLI2LZOlnckwmuJGqYg9cbqAm1YxY3oH.4Ai2wNsWMntq", null, null, null, null, null, null, 2, 9999, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BusinessSettings",
                keyColumn: "Id",
                keyValue: 9999);

            migrationBuilder.DeleteData(
                table: "InvoiceSettings",
                keyColumn: "Id",
                keyValue: 9999);

            migrationBuilder.DeleteData(
                table: "Trials",
                keyColumn: "Id",
                keyValue: 9999);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 9999);

            migrationBuilder.DeleteData(
                table: "Businesses",
                keyColumn: "Id",
                keyValue: 9999);

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
    }
}
