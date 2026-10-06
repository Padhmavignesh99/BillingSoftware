using Microsoft.EntityFrameworkCore;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Models.Enums;
using System.Security.Claims;

namespace BillingSoftware.API.Data
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

        // Theme Settings (per-user)
        public DbSet<UserThemeSettings> UserThemeSettings { get; set; }

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
            modelBuilder.Entity<UserThemeSettings>().HasQueryFilter(t => _tenantId == null || t.TenantId == _tenantId);

            // UserThemeSettings — unique constraint: one row per user per tenant
            modelBuilder.Entity<UserThemeSettings>()
                .HasIndex(t => new { t.UserId, t.TenantId })
                .IsUnique();

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
            // Default credentials: superadmin@billingsoftware.com / SuperAdmin@123
            // IMPORTANT: Change this password after first login!
            modelBuilder.Entity<User>().HasData(new User
            {
                Id             = 1,
                FullName       = "Super Admin",
                Email          = "superadmin@billingsoftware.com",
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
