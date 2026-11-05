using Microsoft.EntityFrameworkCore;
using MasterBilling.API.Models.Entities;
using MasterBilling.API.Models.Enums;
using System.Security.Claims;

namespace MasterBilling.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Current tenant ID resolved from the authenticated user's JWT claims.
        /// Null = SuperAdmin (sees all tenants). Used by global query filters.
        /// </summary>
        private readonly int? _tenantId;

        /// <summary>
        /// Active provider key (e.g. "MySql", "MSSql", "PostgreSQL").
        /// Injected via ApplicationDbContextFactory or Program.cs and stored
        /// so OnModelCreating can emit the correct IS NOT NULL filter syntax.
        /// </summary>
        private readonly string _activeProvider;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IConfiguration configuration,
            IHttpContextAccessor? httpContextAccessor = null)
            : base(options)
        {
            _activeProvider = configuration
                .GetSection("DataSettings:ActiveProvider")
                .Value ?? "MSSql";

            // Resolve current tenant from JWT — SuperAdmin has no TenantId claim (or 0)
            var user = httpContextAccessor?.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var role = user.FindFirst(ClaimTypes.Role)?.Value;
                if (role != nameof(UserRole.SuperAdmin))
                {
                    var tenantClaim = user.FindFirst("TenantId")?.Value;
                    if (int.TryParse(tenantClaim, out var id) && id > 0)
                        _tenantId = id;
                }
                // SuperAdmin: _tenantId stays null → no filter applied
            }
        }

        // Auth & Users
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Staff> Staff { get; set; }

        // Business
        public DbSet<Business> Businesses { get; set; }
        public DbSet<BusinessType> BusinessTypes { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<BusinessSettings> BusinessSettings { get; set; }
        public DbSet<InvoiceSettings> InvoiceSettings { get; set; }

        // Products
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<TaxRate> TaxRates { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        // Inventory
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }
        public DbSet<StockAdjustment> StockAdjustments { get; set; }
        public DbSet<StockTransfer> StockTransfers { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }

        // Purchase
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<PurchasePayment> PurchasePayments { get; set; }
        public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
        public DbSet<PurchaseReturnItem> PurchaseReturnItems { get; set; }
        public DbSet<GoodsReceipt> GoodsReceipts { get; set; }
        public DbSet<GoodsReceiptItem> GoodsReceiptItems { get; set; }

        // Sales
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<SalesReturn> SalesReturns { get; set; }
        public DbSet<SalesReturnItem> SalesReturnItems { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationItem> QuotationItems { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }

        // Customer
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerGroup> CustomerGroups { get; set; }
        public DbSet<CustomerLedger> CustomerLedgers { get; set; }
        public DbSet<CustomerPayment> CustomerPayments { get; set; }
        public DbSet<CustomerCredit> CustomerCredits { get; set; }

        // Supplier
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierGroup> SupplierGroups { get; set; }
        public DbSet<SupplierLedger> SupplierLedgers { get; set; }
        public DbSet<SupplierPayment> SupplierPayments { get; set; }
        public DbSet<SupplierCredit> SupplierCredits { get; set; }

        // Payment
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentMethodEntity> PaymentMethods { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<CashRegister> CashRegisters { get; set; }
        public DbSet<PaymentGatewayTransaction> PaymentGatewayTransactions { get; set; }

        // Expense
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<ExpensePayment> ExpensePayments { get; set; }

        // Communication
        public DbSet<WhatsAppMessage> WhatsAppMessages { get; set; }
        public DbSet<EmailMessage> EmailMessages { get; set; }
        public DbSet<SmsMessage> SmsMessages { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Subscription
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }
        public DbSet<PlanFeature> PlanFeatures { get; set; }
        public DbSet<Trial> Trials { get; set; }

        // System
        public DbSet<FileUpload> FileUploads { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Lookup> Lookups { get; set; }
        public DbSet<BackupRecord> BackupRecords { get; set; }
        public DbSet<PrintSettings> PrintSettings { get; set; }

        // Print Jobs — tracks every print dispatch with status, retry, and reprint support
        public DbSet<PrintJob> PrintJobs { get; set; }

        // Printer Registry — physical/logical printers per tenant/branch/counter
        public DbSet<Printer> Printers { get; set; }

        // Theme Settings (per-user)
        public DbSet<UserThemeSettings> UserThemeSettings { get; set; }

        // ── Multi-tenant Shop Management ──────────────────────────────────────────
        public DbSet<Counter>         Counters           { get; set; }
        public DbSet<EmployeeRole>    EmployeeRoles      { get; set; }
        public DbSet<TenantUserRole>  UserRoles          { get; set; }
        public DbSet<SubscriptionUsage> SubscriptionUsages { get; set; }
        public DbSet<AIInsight>       AIInsights         { get; set; }
        public DbSet<AIForecast>      AIForecasts        { get; set; }
        public DbSet<ShopInvite>      ShopInvites        { get; set; }
        public DbSet<UsageAlert>      UsageAlerts        { get; set; }

        // ── Device Integration Layer ──────────────────────────────────────────────
        /// <summary>Registry of physical devices registered to a billing terminal (scanners, scales, etc.).</summary>
        public DbSet<DeviceConfiguration> DeviceConfigurations { get; set; }

        /// <summary>Per-tenant feature flags that enable/disable each device type.</summary>
        public DbSet<DeviceSettings> DeviceSettings { get; set; }

        /// <summary>Audit log of every barcode scan event across all modules.</summary>
        public DbSet<ScanEventLog> ScanEventLogs { get; set; }

        /// <summary>Log of weight readings captured from connected weighing scales.</summary>
        public DbSet<WeightCaptureLog> WeightCaptureLogs { get; set; }

        /// <summary>Results of supplier invoice / document OCR scans.</summary>
        public DbSet<OcrScanResult> OcrScanResults { get; set; }

        /// <summary>Card/POS terminal payment transactions.</summary>
        public DbSet<CardPaymentTransaction> CardPaymentTransactions { get; set; }

        /// <summary>QR code scan events across all modules.</summary>
        public DbSet<QRScanEvent> QRScanEvents { get; set; }

        // ── New Feature Entities ──────────────────────────────────────────────────
        public DbSet<DeliveryChallan>      DeliveryChallans     { get; set; }
        public DbSet<DeliveryChallanItem>  DeliveryChallanItems { get; set; }
        public DbSet<RecurringInvoice>     RecurringInvoices    { get; set; }
        public DbSet<RecurringInvoiceItem> RecurringInvoiceItems { get; set; }
        public DbSet<StockTakeSession>     StockTakeSessions    { get; set; }
        public DbSet<StockTakeItem>        StockTakeItems       { get; set; }
        public DbSet<LoyaltyProgram>       LoyaltyPrograms      { get; set; }
        public DbSet<LoyaltyTier>          LoyaltyTiers         { get; set; }
        public DbSet<CustomerLoyaltyAccount> CustomerLoyaltyAccounts { get; set; }
        public DbSet<LoyaltyTransaction>   LoyaltyTransactions  { get; set; }
        public DbSet<ReminderRule>         ReminderRules        { get; set; }
        public DbSet<ReminderLog>          ReminderLogs         { get; set; }

        // ── Product Variants ──────────────────────────────────────────────────
        public DbSet<VariantAttribute>        VariantAttributes        { get; set; }
        public DbSet<VariantAttributeValue>   VariantAttributeValues   { get; set; }
        public DbSet<ProductVariant>          ProductVariants          { get; set; }
        public DbSet<ProductVariantAttribute> ProductVariantAttributes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision for all monetary fields
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            // Unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Provider-aware IS NOT NULL filter syntax:
            //   MSSQL      → [ColumnName] IS NOT NULL
            //   PostgreSQL → "ColumnName" IS NOT NULL
            //   MySQL      → ColumnName IS NOT NULL (no quoting needed for simple names)
            string gstFilter, skuFilter, barcodeFilter;

            if (DbContextConfigurator.IsPostgreSQL(_activeProvider))
            {
                gstFilter     = "\"GSTNumber\" IS NOT NULL";
                skuFilter     = "\"SKU\" IS NOT NULL";
                barcodeFilter = "\"Barcode\" IS NOT NULL";
            }
            else if (DbContextConfigurator.IsMySQL(_activeProvider))
            {
                gstFilter     = "`GSTNumber` IS NOT NULL";
                skuFilter     = "`SKU` IS NOT NULL";
                barcodeFilter = "`Barcode` IS NOT NULL";
            }
            else // MSSql (default)
            {
                gstFilter     = "[GSTNumber] IS NOT NULL";
                skuFilter     = "[SKU] IS NOT NULL";
                barcodeFilter = "[Barcode] IS NOT NULL";
            }

            modelBuilder.Entity<Business>()
                .HasIndex(b => b.GSTNumber)
                .IsUnique()
                .HasFilter(gstFilter);

            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.TenantId, p.SKU })
                .IsUnique()
                .HasFilter(skuFilter);

            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.TenantId, p.Barcode })
                .IsUnique()
                .HasFilter(barcodeFilter);

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => new { i.TenantId, i.InvoiceNumber })
                .IsUnique();

            modelBuilder.Entity<Purchase>()
                .HasIndex(p => new { p.TenantId, p.PurchaseNumber })
                .IsUnique();

            // Relationships — StockTransfer has two Branch FKs
            modelBuilder.Entity<StockTransfer>()
                .HasOne(s => s.FromBranch)
                .WithMany()
                .HasForeignKey(s => s.FromBranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTransfer>()
                .HasOne(s => s.ToBranch)
                .WithMany()
                .HasForeignKey(s => s.ToBranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // CashRegister has two User FKs
            modelBuilder.Entity<CashRegister>()
                .HasOne(c => c.OpenedByUser)
                .WithMany()
                .HasForeignKey(c => c.OpenedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CashRegister>()
                .HasOne(c => c.ClosedByUser)
                .WithMany()
                .HasForeignKey(c => c.ClosedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ─── Global query filters: Tenant isolation + soft delete ────────────────
            // TenantId == null means SuperAdmin → no tenant filter (sees everything)
            // For all other roles, every query is automatically scoped to their tenant.
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted && (_tenantId == null || p.TenantId == _tenantId));
            modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted && (_tenantId == null || c.TenantId == _tenantId));
            modelBuilder.Entity<Supplier>().HasQueryFilter(s => !s.IsDeleted && (_tenantId == null || s.TenantId == _tenantId));
            modelBuilder.Entity<Invoice>().HasQueryFilter(i => !i.IsDeleted && (_tenantId == null || i.TenantId == _tenantId));
            modelBuilder.Entity<Purchase>().HasQueryFilter(p => !p.IsDeleted && (_tenantId == null || p.TenantId == _tenantId));
            modelBuilder.Entity<Expense>().HasQueryFilter(e => !e.IsDeleted && (_tenantId == null || e.TenantId == _tenantId));
            // Additional tenant-scoped entities (no soft delete needed)
            modelBuilder.Entity<Branch>().HasQueryFilter(b => _tenantId == null || b.TenantId == _tenantId);
            modelBuilder.Entity<Staff>().HasQueryFilter(s => _tenantId == null || s.TenantId == _tenantId);
            modelBuilder.Entity<Inventory>().HasQueryFilter(i => _tenantId == null || i.TenantId == _tenantId);
            modelBuilder.Entity<Quotation>().HasQueryFilter(q => _tenantId == null || q.TenantId == _tenantId);
            modelBuilder.Entity<SalesOrder>().HasQueryFilter(s => _tenantId == null || s.TenantId == _tenantId);
            modelBuilder.Entity<PurchaseOrder>().HasQueryFilter(p => _tenantId == null || p.TenantId == _tenantId);
            modelBuilder.Entity<PrintSettings>().HasQueryFilter(ps => _tenantId == null || ps.TenantId == _tenantId);
            modelBuilder.Entity<PrintJob>().HasQueryFilter(pj => _tenantId == null || pj.TenantId == _tenantId);
            modelBuilder.Entity<Printer>().HasQueryFilter(p => _tenantId == null || p.TenantId == _tenantId);
            modelBuilder.Entity<UserThemeSettings>().HasQueryFilter(t => _tenantId == null || t.TenantId == _tenantId);

            // ── Device Integration Layer — tenant-scoped filters ───────────────────
            modelBuilder.Entity<DeviceConfiguration>().HasQueryFilter(d => _tenantId == null || d.TenantId == _tenantId);
            modelBuilder.Entity<DeviceSettings>().HasQueryFilter(d => _tenantId == null || d.TenantId == _tenantId);
            modelBuilder.Entity<ScanEventLog>().HasQueryFilter(s => _tenantId == null || s.TenantId == _tenantId);
            modelBuilder.Entity<WeightCaptureLog>().HasQueryFilter(w => _tenantId == null || w.TenantId == _tenantId);
            modelBuilder.Entity<OcrScanResult>().HasQueryFilter(o => _tenantId == null || o.TenantId == _tenantId);
            modelBuilder.Entity<CardPaymentTransaction>().HasQueryFilter(c => _tenantId == null || c.TenantId == _tenantId);
            modelBuilder.Entity<QRScanEvent>().HasQueryFilter(q => _tenantId == null || q.TenantId == _tenantId);

            // DeviceSettings — unique constraint: one settings record per tenant
            modelBuilder.Entity<DeviceSettings>()
                .HasIndex(d => d.TenantId)
                .IsUnique();

            // UserThemeSettings — unique constraint: one row per user per tenant
            modelBuilder.Entity<UserThemeSettings>()
                .HasIndex(t => new { t.UserId, t.TenantId })
                .IsUnique();

            // PrintSettings — unique constraint per (TenantId, BranchId, CounterId).
            // This supports the three-level configuration hierarchy:
            //   • TenantId only         → tenant-level default
            //   • TenantId + BranchId   → branch-level override
            //   • TenantId + BranchId + CounterId → counter-level override
            modelBuilder.Entity<PrintSettings>()
                .HasIndex(ps => new { ps.TenantId, ps.BranchId, ps.CounterId })
                .IsUnique();

            // PrintJob — index for efficient per-tenant status queries and reprint lookups
            modelBuilder.Entity<PrintJob>()
                .HasIndex(pj => new { pj.TenantId, pj.Status });

            modelBuilder.Entity<PrintJob>()
                .HasIndex(pj => new { pj.TenantId, pj.DocumentType, pj.DocumentId });

            modelBuilder.Entity<PrintJob>()
                .HasOne(pj => pj.InitiatedByUser)
                .WithMany()
                .HasForeignKey(pj => pj.InitiatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // PrintSettings branch FK (optional)
            modelBuilder.Entity<PrintSettings>()
                .HasOne(ps => ps.Branch)
                .WithMany()
                .HasForeignKey(ps => ps.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Printer Registry — index for fast per-tenant queries by type and scope
            modelBuilder.Entity<Printer>()
                .HasIndex(p => new { p.TenantId, p.PrinterType });

            modelBuilder.Entity<Printer>()
                .HasIndex(p => new { p.TenantId, p.BranchId, p.CounterId });

            // Category self-referencing relationship
            modelBuilder.Entity<ProductCategory>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Expense category self-referencing
            modelBuilder.Entity<ExpenseCategory>()
                .HasOne(c => c.ParentCategory)
                .WithMany()
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent cascade delete issues
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Customer)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Purchases)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed default business types
            modelBuilder.Entity<BusinessType>().HasData(
                new BusinessType { Id = 1, Name = "Retail Store", Description = "General retail shop", IsActive = true },
                new BusinessType { Id = 2, Name = "Wholesale", Description = "Wholesale business", IsActive = true },
                new BusinessType { Id = 3, Name = "Restaurant", Description = "Food & beverage", IsActive = true },
                new BusinessType { Id = 4, Name = "Pharmacy", Description = "Medical store", IsActive = true },
                new BusinessType { Id = 5, Name = "Electronics", Description = "Electronics shop", IsActive = true },
                new BusinessType { Id = 6, Name = "Clothing", Description = "Fashion & apparel", IsActive = true },
                new BusinessType { Id = 7, Name = "Grocery", Description = "Grocery & supermarket", IsActive = true },
                new BusinessType { Id = 8, Name = "Service", Description = "Service-based business", IsActive = true },
                new BusinessType { Id = 9, Name = "Manufacturing", Description = "Manufacturing unit", IsActive = true },
                new BusinessType { Id = 10, Name = "Other", Description = "Other business type", IsActive = true }
            );

            // ─── Seed SuperAdmin user ─────────────────────────────────────────────────
            // SuperAdmin has no TenantId — full platform access.
            // Default credentials: superadmin@MasterBilling.com / SuperAdmin@123
            // IMPORTANT: Change this password after first login!
            modelBuilder.Entity<User>().HasData(new User
            {
                Id             = 1,
                FullName       = "Super Admin",
                Email          = "superadmin@MasterBilling.com",
                PasswordHash   = BCrypt.Net.BCrypt.HashPassword("SuperAdmin@123"),
                Role           = UserRole.SuperAdmin,
                TenantId       = null,
                IsActive       = true,
                IsDeleted      = false,
                CreatedAt      = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            // Seed default lookup data
            modelBuilder.Entity<Lookup>().HasData(
                new Lookup { Id = 1, Category = "Currency", Key = "INR", Value = "₹", Description = "Indian Rupee" },
                new Lookup { Id = 2, Category = "Currency", Key = "USD", Value = "$", Description = "US Dollar" },
                new Lookup { Id = 3, Category = "Currency", Key = "EUR", Value = "€", Description = "Euro" },
                new Lookup { Id = 4, Category = "Country", Key = "IN", Value = "India", Description = "India" },
                new Lookup { Id = 5, Category = "Country", Key = "US", Value = "United States", Description = "United States" }
            );

            // ─── Seed Admin user ──────────────────────────────────────────────────────
            // Admin credentials: admin@MasterBilling.com / Admin@123
            modelBuilder.Entity<User>().HasData(new User
            {
                Id             = 2,
                FullName       = "Admin",
                Email          = "admin@MasterBilling.com",
                PasswordHash   = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role           = UserRole.SuperAdmin,
                TenantId       = null,
                IsActive       = true,
                IsDeleted      = false,
                CreatedAt      = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            // ─── Seed Demo Business (Vignesh Tex) ────────────────────────────────────
            modelBuilder.Entity<Business>().HasData(new Business
            {
                Id           = 9999,
                Name         = "Vignesh Tex",
                Email        = "padhmavignesh99@gmail.com",
                CurrencyCode = "INR",
                CurrencySymbol = "₹",
                Status       = BusinessStatus.Active,
                IsActive     = true,
                IsDeleted    = false,
                CreatedAt    = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            // ─── Seed Demo Admin User ─────────────────────────────────────────────────
            // Demo credentials: padhmavignesh99@gmail.com / Vignesh@123
            modelBuilder.Entity<User>().HasData(new User
            {
                Id             = 9999,
                FullName       = "Vignesh",
                Email          = "padhmavignesh99@gmail.com",
                PasswordHash   = BCrypt.Net.BCrypt.HashPassword("Vignesh@123"),
                Role           = UserRole.Owner,
                TenantId       = 9999,
                IsActive       = true,
                IsDeleted      = false,
                CreatedAt      = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            // ─── Seed Demo Business Settings ──────────────────────────────────────────
            modelBuilder.Entity<BusinessSettings>().HasData(new BusinessSettings
            {
                Id       = 9999,
                TenantId = 9999,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            modelBuilder.Entity<InvoiceSettings>().HasData(new InvoiceSettings
            {
                Id       = 9999,
                TenantId = 9999,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            // ─── Seed Demo Trial ──────────────────────────────────────────────────────
            modelBuilder.Entity<Trial>().HasData(new Trial
            {
                Id         = 9999,
                BusinessId = 9999,
                StartDate  = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate    = new DateTime(2024, 1, 31, 0, 0, 0, DateTimeKind.Utc),
                IsConverted = false,
                IsActive   = true,
                IsDeleted  = false,
                CreatedAt  = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            // ─── Seed Subscription Plans ──────────────────────────────────────────────
            // Plan 1: Starter (Free Trial)
            // Plan 2: Basic
            // Plan 3: Professional (Popular)
            // Plan 4: Enterprise
            modelBuilder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan
                {
                    Id = 1, Name = "Starter", Description = "Perfect for small shops getting started. Includes a 30-day free trial.",
                    MonthlyPrice = 0, YearlyPrice = 0,
                    MaxProducts = 50, MaxUsers = 2, MaxBranches = 1, MaxInvoicesPerMonth = 100,
                    HasWhatsAppIntegration = false, HasSmsIntegration = false, HasEmailIntegration = true, HasReports = false,
                    HasMultiCurrency = false, HasInventoryManagement = true, HasDeviceIntegration = false, HasApiAccess = false,
                    TrialDays = 30, IsActive = true, IsPopular = false, SortOrder = 1,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new SubscriptionPlan
                {
                    Id = 2, Name = "Basic", Description = "Ideal for growing businesses with essential billing features.",
                    MonthlyPrice = 499, YearlyPrice = 4999,
                    MaxProducts = 500, MaxUsers = 5, MaxBranches = 1, MaxInvoicesPerMonth = 1000,
                    HasWhatsAppIntegration = false, HasSmsIntegration = false, HasEmailIntegration = true, HasReports = true,
                    HasMultiCurrency = false, HasInventoryManagement = true, HasDeviceIntegration = false, HasApiAccess = false,
                    TrialDays = 30, IsActive = true, IsPopular = false, SortOrder = 2,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new SubscriptionPlan
                {
                    Id = 3, Name = "Professional", Description = "For established businesses needing advanced features and integrations.",
                    MonthlyPrice = 999, YearlyPrice = 9999,
                    MaxProducts = 5000, MaxUsers = 15, MaxBranches = 3, MaxInvoicesPerMonth = 10000,
                    HasWhatsAppIntegration = true, HasSmsIntegration = true, HasEmailIntegration = true, HasReports = true,
                    HasMultiCurrency = true, HasInventoryManagement = true, HasDeviceIntegration = true, HasApiAccess = false,
                    TrialDays = 30, IsActive = true, IsPopular = true, SortOrder = 3,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new SubscriptionPlan
                {
                    Id = 4, Name = "Enterprise", Description = "Unlimited features for large enterprises with API access and dedicated support.",
                    MonthlyPrice = 2499, YearlyPrice = 24999,
                    MaxProducts = int.MaxValue, MaxUsers = int.MaxValue, MaxBranches = int.MaxValue, MaxInvoicesPerMonth = int.MaxValue,
                    HasWhatsAppIntegration = true, HasSmsIntegration = true, HasEmailIntegration = true, HasReports = true,
                    HasMultiCurrency = true, HasInventoryManagement = true, HasDeviceIntegration = true, HasApiAccess = true,
                    TrialDays = 30, IsActive = true, IsPopular = false, SortOrder = 4,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // ─── Seed Plan Features ───────────────────────────────────────────────────
            modelBuilder.Entity<PlanFeature>().HasData(
                // Starter features
                new PlanFeature { Id = 1,  PlanId = 1, FeatureName = "Sales Invoice",        IsEnabled = true,  Description = "Create and manage sales invoices", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                new PlanFeature { Id = 2,  PlanId = 1, FeatureName = "Basic Inventory",       IsEnabled = true,  Description = "Track stock for up to 50 products", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                new PlanFeature { Id = 3,  PlanId = 1, FeatureName = "Customer Management",   IsEnabled = true,  Description = "Manage customer records", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                // Basic features
                new PlanFeature { Id = 4,  PlanId = 2, FeatureName = "Sales & Purchase",      IsEnabled = true,  Description = "Full sales and purchase management", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                new PlanFeature { Id = 5,  PlanId = 2, FeatureName = "Reports",               IsEnabled = true,  Description = "Sales, purchase, and stock reports", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                new PlanFeature { Id = 6,  PlanId = 2, FeatureName = "GST Reports",           IsEnabled = true,  Description = "GSTR-1, GSTR-3B summary reports", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                new PlanFeature { Id = 7,  PlanId = 2, FeatureName = "Email Integration",     IsEnabled = true,  Description = "Send invoices via email", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                // Professional features
                new PlanFeature { Id = 8,  PlanId = 3, FeatureName = "WhatsApp Integration",  IsEnabled = true,  Description = "Send invoices & reminders via WhatsApp", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                new PlanFeature { Id = 9,  PlanId = 3, FeatureName = "Multi-Branch",          IsEnabled = true,  Description = "Manage up to 3 branches", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                new PlanFeature { Id = 10, PlanId = 3, FeatureName = "Device Integration",    IsEnabled = true,  Description = "Barcode scanners, weighing scales, card terminals", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                new PlanFeature { Id = 11, PlanId = 3, FeatureName = "Multi-Currency",        IsEnabled = true,  Description = "Accept payments in multiple currencies", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                new PlanFeature { Id = 12, PlanId = 3, FeatureName = "Advanced Reports",      IsEnabled = true,  Description = "Profit & loss, customer analytics, trend reports", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                // Enterprise features
                new PlanFeature { Id = 13, PlanId = 4, FeatureName = "Unlimited Everything",  IsEnabled = true,  Description = "No limits on products, users, or branches", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                new PlanFeature { Id = 14, PlanId = 4, FeatureName = "API Access",            IsEnabled = true,  Description = "REST API access for custom integrations", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                new PlanFeature { Id = 15, PlanId = 4, FeatureName = "Dedicated Support",     IsEnabled = true,  Description = "Priority support with dedicated account manager", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                new PlanFeature { Id = 16, PlanId = 4, FeatureName = "Custom Integrations",   IsEnabled = true,  Description = "Custom ERP/CRM integrations on request", CreatedAt = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) }
            );

            // ─── Multi-Tenant Management Relationships ────────────────────────────────

            // EmployeeRole → RolePermission (separate from system Role→RolePermission)
            modelBuilder.Entity<EmployeeRole>()
                .HasMany(er => er.Permissions)
                .WithOne()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore the PermissionDef alias navigation (same FK as Permission)
            modelBuilder.Entity<RolePermission>()
                .Ignore(rp => rp.PermissionDef);

            // TenantUserRole — multiple User FKs
            modelBuilder.Entity<TenantUserRole>()
                .HasOne(ur => ur.User)
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TenantUserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(er => er.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TenantUserRole>()
                .HasOne(ur => ur.Branch)
                .WithMany()
                .HasForeignKey(ur => ur.BranchId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TenantUserRole>()
                .HasOne(ur => ur.Counter)
                .WithMany()
                .HasForeignKey(ur => ur.CounterId)
                .OnDelete(DeleteBehavior.SetNull);

            // ShopInvite — three User FKs
            modelBuilder.Entity<ShopInvite>()
                .HasOne(i => i.InvitedByUser)
                .WithMany()
                .HasForeignKey(i => i.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ShopInvite>()
                .HasOne(i => i.AcceptedByUser)
                .WithMany()
                .HasForeignKey(i => i.AcceptedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Counter — AssignedUser FK
            modelBuilder.Entity<Counter>()
                .HasOne(c => c.AssignedUser)
                .WithMany()
                .HasForeignKey(c => c.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Warehouse — ignore ambiguous collection navs, configure via StockTransfer side
            modelBuilder.Entity<Warehouse>()
                .Ignore(w => w.InboundTransfers)
                .Ignore(w => w.OutboundTransfers);

            // StockTransfer — explicit warehouse FKs
            modelBuilder.Entity<StockTransfer>()
                .HasOne(s => s.FromWarehouse)
                .WithMany()
                .HasForeignKey(s => s.FromWarehouseId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<StockTransfer>()
                .HasOne(s => s.ToWarehouse)
                .WithMany()
                .HasForeignKey(s => s.ToWarehouseId)
                .OnDelete(DeleteBehavior.SetNull);

            // AIForecast product FK
            modelBuilder.Entity<AIForecast>()
                .HasOne(f => f.Product)
                .WithMany()
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // AIInsight generated-by user FK
            modelBuilder.Entity<AIInsight>()
                .HasOne(i => i.GeneratedByUser)
                .WithMany()
                .HasForeignKey(i => i.GeneratedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // SubscriptionUsage FK
            modelBuilder.Entity<SubscriptionUsage>()
                .HasOne(u => u.Subscription)
                .WithMany()
                .HasForeignKey(u => u.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tenant-scoped query filters for new entities
            modelBuilder.Entity<Counter>()         .HasQueryFilter(c => !c.IsDeleted && (_tenantId == null || c.TenantId == _tenantId));
            modelBuilder.Entity<EmployeeRole>()    .HasQueryFilter(r => !r.IsDeleted && (_tenantId == null || r.TenantId == _tenantId));
            modelBuilder.Entity<TenantUserRole>()  .HasQueryFilter(r => !r.IsDeleted && (_tenantId == null || r.TenantId == _tenantId));
            modelBuilder.Entity<SubscriptionUsage>().HasQueryFilter(u => !u.IsDeleted && (_tenantId == null || u.TenantId == _tenantId));
            modelBuilder.Entity<AIInsight>()       .HasQueryFilter(i => !i.IsDeleted && (_tenantId == null || i.TenantId == _tenantId));
            modelBuilder.Entity<AIForecast>()      .HasQueryFilter(f => !f.IsDeleted && (_tenantId == null || f.TenantId == _tenantId));
            modelBuilder.Entity<ShopInvite>()      .HasQueryFilter(i => !i.IsDeleted && (_tenantId == null || i.TenantId == _tenantId));
            modelBuilder.Entity<UsageAlert>()      .HasQueryFilter(a => !a.IsDeleted && (_tenantId == null || a.TenantId == _tenantId));
            modelBuilder.Entity<Warehouse>()       .HasQueryFilter(w => !w.IsDeleted && (_tenantId == null || w.TenantId == _tenantId));

        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
