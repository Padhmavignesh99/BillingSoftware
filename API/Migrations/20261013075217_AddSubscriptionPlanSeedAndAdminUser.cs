using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MasterBilling.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionPlanSeedAndAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasApiAccess",
                table: "SubscriptionPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasDeviceIntegration",
                table: "SubscriptionPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TrialDays",
                table: "SubscriptionPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspendedAt",
                table: "Businesses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspendedReason",
                table: "Businesses",
                type: "text",
                nullable: true);

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

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "HasApiAccess", "HasDeviceIntegration", "HasEmailIntegration", "HasInventoryManagement", "HasMultiCurrency", "HasReports", "HasWhatsAppIntegration", "IsActive", "IsDeleted", "IsPopular", "MaxBranches", "MaxInvoicesPerMonth", "MaxProducts", "MaxUsers", "MonthlyPrice", "Name", "SortOrder", "TrialDays", "UpdatedAt", "UpdatedBy", "YearlyPrice" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Perfect for small shops getting started. Includes a 30-day free trial.", false, false, true, true, false, false, false, true, false, false, 1, 100, 50, 2, 0m, "Starter", 1, 30, null, null, 0m },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ideal for growing businesses with essential billing features.", false, false, true, true, false, true, false, true, false, false, 1, 1000, 500, 5, 499m, "Basic", 2, 30, null, null, 4999m },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "For established businesses needing advanced features and integrations.", false, true, true, true, true, true, true, true, false, true, 3, 10000, 5000, 15, 999m, "Professional", 3, 30, null, null, 9999m },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Unlimited features for large enterprises with API access and dedicated support.", true, true, true, true, true, true, true, true, false, false, 2147483647, 2147483647, 2147483647, 2147483647, 2499m, "Enterprise", 4, 30, null, null, 24999m }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$RLMFfLHpCtWcQ6NywaIEQ.i//uZafD2wvYdhwr7iaRkuRJn3VG9.2");

            migrationBuilder.Sql(@"
                INSERT INTO ""Users"" (""Id"", ""CreatedAt"", ""CreatedBy"", ""Email"", ""FullName"", ""IsActive"", ""IsDeleted"",
                    ""LastLoginAt"", ""OTP"", ""OTPExpiry"", ""PasswordHash"", ""Phone"", ""ProfileImageUrl"",
                    ""RefreshToken"", ""RefreshTokenExpiry"", ""ResetToken"", ""ResetTokenExpiry"", ""Role"", ""TenantId"", ""UpdatedAt"", ""UpdatedBy"")
                VALUES (2, TIMESTAMPTZ '2024-01-01T00:00:00Z', NULL, 'admin@MasterBilling.com', 'Admin',
                    TRUE, FALSE, NULL, NULL, NULL, '$2a$11$Y75qrPxbRO2d3VAlUkhYuur0.vaeKXvhhpdZOEgyJ5PJ4SgPH/HaS',
                    NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL)
                ON CONFLICT (""Id"") DO NOTHING;");


            migrationBuilder.InsertData(
                table: "PlanFeatures",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "FeatureName", "FeatureValue", "IsActive", "IsDeleted", "IsEnabled", "PlanId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create and manage sales invoices", "Sales Invoice", null, true, false, true, 1, null, null },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Track stock for up to 50 products", "Basic Inventory", null, true, false, true, 1, null, null },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Manage customer records", "Customer Management", null, true, false, true, 1, null, null },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Full sales and purchase management", "Sales & Purchase", null, true, false, true, 2, null, null },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Sales, purchase, and stock reports", "Reports", null, true, false, true, 2, null, null },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "GSTR-1, GSTR-3B summary reports", "GST Reports", null, true, false, true, 2, null, null },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Send invoices via email", "Email Integration", null, true, false, true, 2, null, null },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Send invoices & reminders via WhatsApp", "WhatsApp Integration", null, true, false, true, 3, null, null },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Manage up to 3 branches", "Multi-Branch", null, true, false, true, 3, null, null },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Barcode scanners, weighing scales, card terminals", "Device Integration", null, true, false, true, 3, null, null },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Accept payments in multiple currencies", "Multi-Currency", null, true, false, true, 3, null, null },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Profit & loss, customer analytics, trend reports", "Advanced Reports", null, true, false, true, 3, null, null },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "No limits on products, users, or branches", "Unlimited Everything", null, true, false, true, 4, null, null },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "REST API access for custom integrations", "API Access", null, true, false, true, 4, null, null },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Priority support with dedicated account manager", "Dedicated Support", null, true, false, true, 4, null, null },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Custom ERP/CRM integrations on request", "Custom Integrations", null, true, false, true, 4, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "PlanFeatures",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "HasApiAccess",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "HasDeviceIntegration",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "TrialDays",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "SuspendedAt",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "SuspendedReason",
                table: "Businesses");

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
    }
}
