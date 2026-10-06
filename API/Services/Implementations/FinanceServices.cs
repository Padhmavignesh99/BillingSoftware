using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Models.Enums;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingSoftware.API.Services.Implementations
{
    // ==================== PAYMENT SERVICE ====================
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        public PaymentService(ApplicationDbContext context) { _context = context; }

        public async Task<(List<PaymentResponse> Items, int TotalCount)> GetAllPaymentsAsync(int tenantId, PaginationRequest pagination)
        {
            var query = _context.Payments.Include(p => p.Invoice).Include(p => p.Customer).Where(p => p.TenantId == tenantId);
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(p => p.PaymentDate).Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize)
                .Select(p => new PaymentResponse { Id = p.Id, InvoiceId = p.InvoiceId, InvoiceNumber = p.Invoice != null ? p.Invoice.InvoiceNumber : null, CustomerName = p.Customer != null ? p.Customer.Name : null, Amount = p.Amount, PaymentMethod = p.PaymentMethod.ToString(), Status = p.Status.ToString(), PaymentDate = p.PaymentDate, ReferenceNumber = p.ReferenceNumber, TransactionId = p.TransactionId }).ToListAsync();
            return (items, total);
        }

        public async Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, int tenantId)
        {
            var payment = new Payment { InvoiceId = request.InvoiceId, CustomerId = request.CustomerId, Amount = request.Amount, PaymentMethod = request.PaymentMethod, ReferenceNumber = request.ReferenceNumber, TransactionId = request.TransactionId, Notes = request.Notes, BranchId = request.BranchId, TenantId = tenantId };
            _context.Payments.Add(payment);

            if (request.InvoiceId.HasValue)
            {
                var invoice = await _context.Invoices.FindAsync(request.InvoiceId);
                if (invoice != null)
                {
                    invoice.PaidAmount += request.Amount;
                    invoice.DueAmount = invoice.TotalAmount - invoice.PaidAmount;
                    invoice.Status = invoice.DueAmount <= 0 ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
                }
            }
            await _context.SaveChangesAsync();
            return new PaymentResponse { Id = payment.Id, InvoiceId = payment.InvoiceId, Amount = payment.Amount, PaymentMethod = payment.PaymentMethod.ToString(), Status = payment.Status.ToString(), PaymentDate = payment.PaymentDate, ReferenceNumber = payment.ReferenceNumber };
        }

        public async Task<List<PaymentResponse>> GetDueListAsync(int tenantId) =>
            await _context.Invoices.Include(i => i.Customer).Where(i => i.TenantId == tenantId && i.DueAmount > 0 && i.Status != InvoiceStatus.Cancelled)
                .Select(i => new PaymentResponse { InvoiceId = i.Id, InvoiceNumber = i.InvoiceNumber, CustomerName = i.Customer != null ? i.Customer.Name : "Walk-in", Amount = i.DueAmount, PaymentDate = i.DueDate ?? i.InvoiceDate, Status = "Due" }).ToListAsync();

        public async Task<PaymentResponse> CollectDueAsync(CollectDueRequest request, int tenantId)
        {
            return await CreatePaymentAsync(new CreatePaymentRequest { InvoiceId = request.InvoiceId, CustomerId = request.CustomerId, Amount = request.Amount, PaymentMethod = request.PaymentMethod, ReferenceNumber = request.ReferenceNumber, Notes = request.Notes }, tenantId);
        }
    }

    // ==================== EXPENSE SERVICE ====================
    public class ExpenseService : IExpenseService
    {
        private readonly ApplicationDbContext _context;
        public ExpenseService(ApplicationDbContext context) { _context = context; }

        public async Task<(List<ExpenseResponse> Items, int TotalCount)> GetAllExpensesAsync(int tenantId, PaginationRequest pagination)
        {
            var query = _context.Expenses.Include(e => e.ExpenseCategory).Where(e => e.TenantId == tenantId);
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(e => e.ExpenseDate).Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize)
                .Select(e => new ExpenseResponse { Id = e.Id, ExpenseNumber = e.ExpenseNumber, CategoryName = e.ExpenseCategory != null ? e.ExpenseCategory.Name : null, Amount = e.Amount, ExpenseDate = e.ExpenseDate, Description = e.Description, PaymentMethodName = e.PaymentMethod.HasValue ? e.PaymentMethod.ToString() : null, PaidTo = e.PaidTo, Status = e.Status }).ToListAsync();
            return (items, total);
        }

        public async Task<ExpenseResponse?> GetExpenseByIdAsync(int id, int tenantId)
        {
            var e = await _context.Expenses.Include(e => e.ExpenseCategory).FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);
            return e == null ? null : new ExpenseResponse { Id = e.Id, ExpenseNumber = e.ExpenseNumber, CategoryName = e.ExpenseCategory?.Name, Amount = e.Amount, ExpenseDate = e.ExpenseDate, Description = e.Description, PaymentMethodName = e.PaymentMethod?.ToString(), PaidTo = e.PaidTo, Status = e.Status };
        }

        public async Task<ExpenseResponse> CreateExpenseAsync(CreateExpenseRequest request, int tenantId)
        {
            var count = await _context.Expenses.CountAsync(e => e.TenantId == tenantId);
            var expense = new Expense { ExpenseNumber = $"EXP-{DateTime.UtcNow:yyyy}-{(count + 1):D5}", ExpenseCategoryId = request.ExpenseCategoryId, Amount = request.Amount, ExpenseDate = request.ExpenseDate ?? DateTime.UtcNow, Description = request.Description, ReferenceNumber = request.ReferenceNumber, PaymentMethod = request.PaymentMethod, PaidTo = request.PaidTo, BranchId = request.BranchId, Notes = request.Notes, TenantId = tenantId };
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            return (await GetExpenseByIdAsync(expense.Id, tenantId))!;
        }

        public async Task<ExpenseResponse> UpdateExpenseAsync(int id, UpdateExpenseRequest request, int tenantId)
        {
            var e = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId) ?? throw new KeyNotFoundException("Expense not found");
            if (request.ExpenseCategoryId.HasValue) e.ExpenseCategoryId = request.ExpenseCategoryId;
            if (request.Amount.HasValue) e.Amount = request.Amount.Value;
            if (request.ExpenseDate.HasValue) e.ExpenseDate = request.ExpenseDate.Value;
            if (request.Description != null) e.Description = request.Description;
            if (request.PaymentMethod.HasValue) e.PaymentMethod = request.PaymentMethod;
            if (request.PaidTo != null) e.PaidTo = request.PaidTo;
            await _context.SaveChangesAsync();
            return (await GetExpenseByIdAsync(id, tenantId))!;
        }

        public async Task<bool> DeleteExpenseAsync(int id, int tenantId)
        { var e = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId) ?? throw new KeyNotFoundException("Expense not found"); e.IsDeleted = true; await _context.SaveChangesAsync(); return true; }
    }

    // ==================== DASHBOARD SERVICE ====================
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        public DashboardService(ApplicationDbContext context) { _context = context; }

        public async Task<DashboardSummaryResponse> GetSummaryAsync(int tenantId)
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            return new DashboardSummaryResponse
            {
                TodaySales = await _context.Invoices.Where(i => i.TenantId == tenantId && i.InvoiceDate.Date == today && i.Status != InvoiceStatus.Cancelled).SumAsync(i => i.TotalAmount),
                TodayExpenses = await _context.Expenses.Where(e => e.TenantId == tenantId && e.ExpenseDate.Date == today).SumAsync(e => e.Amount),
                TodayInvoiceCount = await _context.Invoices.CountAsync(i => i.TenantId == tenantId && i.InvoiceDate.Date == today && i.Status != InvoiceStatus.Cancelled),
                TotalRevenue = await _context.Invoices.Where(i => i.TenantId == tenantId && i.Status == InvoiceStatus.Paid).SumAsync(i => i.TotalAmount),
                TotalDueAmount = await _context.Invoices.Where(i => i.TenantId == tenantId && i.DueAmount > 0).SumAsync(i => i.DueAmount),
                TotalCustomers = await _context.Customers.CountAsync(c => c.TenantId == tenantId),
                TotalProducts = await _context.Products.CountAsync(p => p.TenantId == tenantId),
                LowStockProductCount = await _context.Products.CountAsync(p => p.TenantId == tenantId && p.MinStockLevel.HasValue && p.CurrentStock <= p.MinStockLevel),
                MonthlyRevenue = await _context.Invoices.Where(i => i.TenantId == tenantId && i.InvoiceDate >= monthStart && i.Status != InvoiceStatus.Cancelled).SumAsync(i => i.TotalAmount),
                MonthlyExpenses = await _context.Expenses.Where(e => e.TenantId == tenantId && e.ExpenseDate >= monthStart).SumAsync(e => e.Amount)
            };
        }

        public async Task<DashboardChartResponse> GetChartsAsync(int tenantId, DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var to = toDate ?? DateTime.UtcNow;

            var salesData = await _context.Invoices.Where(i => i.TenantId == tenantId && i.InvoiceDate >= from && i.InvoiceDate <= to && i.Status != InvoiceStatus.Cancelled)
                .GroupBy(i => i.InvoiceDate.Date).Select(g => new ChartDataPoint { Label = g.Key.ToString("dd MMM"), Value = g.Sum(i => i.TotalAmount) }).ToListAsync();

            var topProducts = await _context.InvoiceItems.Include(ii => ii.Invoice).Where(ii => ii.Invoice!.TenantId == tenantId && ii.Invoice.InvoiceDate >= from && ii.Invoice.InvoiceDate <= to)
                .GroupBy(ii => ii.ProductName).Select(g => new TopProductResponse { ProductName = g.Key, TotalSold = g.Sum(ii => ii.Quantity), Revenue = g.Sum(ii => ii.TotalPrice) })
                .OrderByDescending(t => t.Revenue).Take(10).ToListAsync();

            return new DashboardChartResponse { SalesChart = salesData, TopProducts = topProducts };
        }
    }

    // ==================== REPORT SERVICE ====================
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        public ReportService(ApplicationDbContext context) { _context = context; }

        public async Task<SalesSummaryResponse> GetSalesSummaryAsync(int tenantId, ReportFilterRequest filter)
        {
            var query = _context.Invoices.Where(i => i.TenantId == tenantId && i.Status != InvoiceStatus.Cancelled);
            if (filter.FromDate.HasValue) query = query.Where(i => i.InvoiceDate >= filter.FromDate);
            if (filter.ToDate.HasValue) query = query.Where(i => i.InvoiceDate <= filter.ToDate);
            if (filter.BranchId.HasValue) query = query.Where(i => i.BranchId == filter.BranchId);

            return new SalesSummaryResponse
            {
                TotalSales = await query.SumAsync(i => i.TotalAmount),
                TotalTax = await query.SumAsync(i => i.TaxAmount),
                TotalDiscount = await query.SumAsync(i => i.DiscountAmount),
                TotalInvoices = await query.CountAsync(),
                AverageOrderValue = await query.AnyAsync() ? await query.AverageAsync(i => i.TotalAmount) : 0
            };
        }

        public async Task<ProfitLossResponse> GetProfitLossAsync(int tenantId, ReportFilterRequest filter)
        {
            var revenue = await _context.Invoices.Where(i => i.TenantId == tenantId && i.Status != InvoiceStatus.Cancelled).SumAsync(i => i.TotalAmount);
            var expenses = await _context.Expenses.Where(e => e.TenantId == tenantId).SumAsync(e => e.Amount);
            var costOfGoods = await _context.PurchaseItems.Include(pi => pi.Purchase).Where(pi => pi.Purchase!.TenantId == tenantId).SumAsync(pi => pi.TotalPrice);
            var grossProfit = revenue - costOfGoods;
            return new ProfitLossResponse { TotalRevenue = revenue, TotalCostOfGoods = costOfGoods, GrossProfit = grossProfit, TotalExpenses = expenses, NetProfit = grossProfit - expenses, ProfitMargin = revenue > 0 ? (grossProfit - expenses) / revenue * 100 : 0 };
        }

        public async Task<TaxReportResponse> GetTaxReportAsync(int tenantId, ReportFilterRequest filter)
        {
            var query = _context.InvoiceItems.Include(ii => ii.Invoice).Where(ii => ii.Invoice!.TenantId == tenantId && ii.Invoice.Status != InvoiceStatus.Cancelled);
            return new TaxReportResponse
            {
                TotalTaxCollected = await query.SumAsync(ii => ii.TaxAmount),
                TotalCGST = await query.SumAsync(ii => ii.CGSTAmount ?? 0),
                TotalSGST = await query.SumAsync(ii => ii.SGSTAmount ?? 0),
                TotalIGST = await query.SumAsync(ii => ii.IGSTAmount ?? 0)
            };
        }

        public async Task<StockReportResponse> GetStockReportAsync(int tenantId, ReportFilterRequest filter)
        {
            var products = await _context.Products.Where(p => p.TenantId == tenantId).ToListAsync();
            return new StockReportResponse
            {
                TotalProducts = products.Count,
                InStockProducts = products.Count(p => p.CurrentStock > 0),
                LowStockProducts = products.Count(p => p.MinStockLevel.HasValue && p.CurrentStock <= p.MinStockLevel),
                OutOfStockProducts = products.Count(p => p.CurrentStock <= 0),
                TotalStockValue = products.Sum(p => p.CurrentStock * p.PurchasePrice),
                Items = products.Select(p => new StockItemResponse { ProductId = p.Id, ProductName = p.Name, SKU = p.SKU, CurrentStock = p.CurrentStock, MinStockLevel = p.MinStockLevel, PurchasePrice = p.PurchasePrice, StockValue = p.CurrentStock * p.PurchasePrice, StockStatus = p.CurrentStock <= 0 ? "Out of Stock" : p.MinStockLevel.HasValue && p.CurrentStock <= p.MinStockLevel ? "Low Stock" : "In Stock" }).ToList()
            };
        }

        public async Task<byte[]> ExportReportAsync(int tenantId, ExportReportRequest request) => System.Text.Encoding.UTF8.GetBytes("Report export placeholder");
        public async Task<object> GetInventoryReportAsync(int tenantId, ReportFilterRequest filter) => await GetStockReportAsync(tenantId, filter);
        public async Task<object> GetCustomerReportAsync(int tenantId, ReportFilterRequest filter) => new { TotalCustomers = await _context.Customers.CountAsync(c => c.TenantId == tenantId), TopCustomers = await _context.Customers.Where(c => c.TenantId == tenantId).OrderByDescending(c => c.TotalPurchaseAmount).Take(10).Select(c => new { c.Name, c.TotalPurchaseAmount }).ToListAsync() };
        public async Task<object> GetSupplierReportAsync(int tenantId, ReportFilterRequest filter) => new { TotalSuppliers = await _context.Suppliers.CountAsync(s => s.TenantId == tenantId) };
        public async Task<object> GetExpenseReportAsync(int tenantId, ReportFilterRequest filter) => new { TotalExpenses = await _context.Expenses.Where(e => e.TenantId == tenantId).SumAsync(e => e.Amount) };

        // ── Purchase Report (typed) ──────────────────────────────────────────────
        public async Task<PurchaseReportResponse> GetPurchaseReportAsync(int tenantId, ReportFilterRequest filter)
        {
            var q = _context.Purchases.Include(p => p.Supplier).Where(p => p.TenantId == tenantId);
            if (filter.FromDate.HasValue) q = q.Where(p => p.PurchaseDate >= filter.FromDate);
            if (filter.ToDate.HasValue)   q = q.Where(p => p.PurchaseDate <= filter.ToDate);
            if (filter.SupplierId.HasValue) q = q.Where(p => p.SupplierId == filter.SupplierId);
            if (!string.IsNullOrEmpty(filter.Status)) q = q.Where(p => p.Status.ToString() == filter.Status);
            var list = await q.ToListAsync();
            return new PurchaseReportResponse
            {
                TotalPurchases      = list.Count,
                TotalPurchaseAmount = list.Sum(p => p.TotalAmount),
                TotalTax            = list.Sum(p => p.TaxAmount),
                TotalDiscount       = list.Sum(p => p.DiscountAmount),
                TotalPaid           = list.Sum(p => p.PaidAmount),
                TotalDue            = list.Sum(p => p.DueAmount),
                Items = list.Select(p => new PurchaseReportItem
                {
                    Id = p.Id, PurchaseNumber = p.PurchaseNumber,
                    SupplierName = p.Supplier?.Name, PurchaseDate = p.PurchaseDate,
                    TotalAmount = p.TotalAmount, PaidAmount = p.PaidAmount,
                    DueAmount = p.DueAmount, Status = p.Status.ToString()
                }).ToList()
            };
        }

        // ── Sales Return Report ──────────────────────────────────────────────────
        public async Task<SalesReturnReportResponse> GetSalesReturnReportAsync(int tenantId, ReportFilterRequest filter)
        {
            var q = _context.SalesReturns.Include(r => r.Invoice).Include(r => r.Customer).Where(r => r.TenantId == tenantId);
            if (filter.FromDate.HasValue) q = q.Where(r => r.ReturnDate >= filter.FromDate);
            if (filter.ToDate.HasValue)   q = q.Where(r => r.ReturnDate <= filter.ToDate);
            if (filter.CustomerId.HasValue) q = q.Where(r => r.CustomerId == filter.CustomerId);
            var list = await q.ToListAsync();
            return new SalesReturnReportResponse
            {
                TotalReturns = list.Count,
                TotalReturnAmount = list.Sum(r => r.TotalAmount),
                Items = list.Select(r => new SalesReturnReportItem
                {
                    Id = r.Id, ReturnNumber = r.ReturnNumber,
                    InvoiceNumber = r.Invoice?.InvoiceNumber, CustomerName = r.Customer?.Name,
                    ReturnDate = r.ReturnDate, TotalAmount = r.TotalAmount,
                    Reason = r.Reason, Status = r.Status
                }).ToList()
            };
        }

        // ── Purchase Return Report ───────────────────────────────────────────────
        public async Task<PurchaseReturnReportResponse> GetPurchaseReturnReportAsync(int tenantId, ReportFilterRequest filter)
        {
            var q = _context.PurchaseReturns.Include(r => r.Supplier).Where(r => r.TenantId == tenantId);
            if (filter.FromDate.HasValue) q = q.Where(r => r.ReturnDate >= filter.FromDate);
            if (filter.ToDate.HasValue)   q = q.Where(r => r.ReturnDate <= filter.ToDate);
            if (filter.SupplierId.HasValue) q = q.Where(r => r.SupplierId == filter.SupplierId);
            var list = await q.ToListAsync();
            return new PurchaseReturnReportResponse
            {
                TotalReturns = list.Count,
                TotalReturnAmount = list.Sum(r => r.TotalAmount),
                Items = list.Select(r => new PurchaseReturnReportItem
                {
                    Id = r.Id, ReturnNumber = r.ReturnNumber,
                    SupplierName = r.Supplier?.Name, ReturnDate = r.ReturnDate,
                    TotalAmount = r.TotalAmount, Reason = r.Reason, Status = r.Status
                }).ToList()
            };
        }

        // ── Low Stock Report ─────────────────────────────────────────────────────
        public async Task<LowStockReportResponse> GetLowStockReportAsync(int tenantId)
        {
            var products = await _context.Products.Include(p => p.Category)
                .Where(p => p.TenantId == tenantId && p.MinStockLevel.HasValue && p.CurrentStock <= p.MinStockLevel)
                .ToListAsync();
            return new LowStockReportResponse
            {
                TotalLowStockProducts = products.Count,
                Items = products.Select(p => new LowStockReportItem
                {
                    ProductId = p.Id, ProductName = p.Name, SKU = p.SKU,
                    CategoryName = p.Category?.Name, CurrentStock = p.CurrentStock,
                    MinStockLevel = p.MinStockLevel,
                    ShortageQuantity = (p.MinStockLevel ?? 0) - p.CurrentStock,
                    PurchasePrice = p.PurchasePrice, SellingPrice = p.SellingPrice
                }).ToList()
            };
        }

        // ── Credit Bill Report (user-wise) ───────────────────────────────────────
        public async Task<CreditBillReportResponse> GetCreditBillReportAsync(int tenantId, int? userId = null)
        {
            var q = _context.CustomerCredits.Include(c => c.Customer).Include(c => c.Invoice).Include(c => c.User)
                .Where(c => c.TenantId == tenantId);
            if (userId.HasValue) q = q.Where(c => c.UserId == userId);
            var list = await q.ToListAsync();
            return new CreditBillReportResponse
            {
                TotalCreditBills   = list.Count,
                TotalCreditAmount  = list.Sum(c => c.Amount),
                TotalPaidAmount    = list.Sum(c => c.PaidAmount),
                TotalBalanceAmount = list.Sum(c => c.BalanceAmount),
                Items = list.Select(c => new CreditBillReportItem
                {
                    Id = c.Id, InvoiceNumber = c.Invoice?.InvoiceNumber,
                    CustomerName = c.Customer?.Name ?? string.Empty,
                    CustomerPhone = c.Customer?.Phone,
                    CreditAmount = c.Amount, PaidAmount = c.PaidAmount,
                    BalanceAmount = c.BalanceAmount, DueDate = c.DueDate,
                    Status = c.Status, UserId = c.UserId,
                    UserName = c.User?.FullName
                }).ToList()
            };
        }

        // ── Product Mini Statement ───────────────────────────────────────────────
        public async Task<ProductMiniStatementResponse> GetProductMiniStatementAsync(int tenantId, int productId, ReportFilterRequest filter)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId);
            if (product == null) return new ProductMiniStatementResponse();

            var transactions = new List<ProductTransactionItem>();

            // Sales
            var sales = await _context.InvoiceItems.Include(ii => ii.Invoice).ThenInclude(i => i!.Customer)
                .Where(ii => ii.ProductId == productId && ii.Invoice!.TenantId == tenantId)
                .Where(ii => !filter.FromDate.HasValue || ii.Invoice!.InvoiceDate >= filter.FromDate)
                .Where(ii => !filter.ToDate.HasValue   || ii.Invoice!.InvoiceDate <= filter.ToDate)
                .ToListAsync();
            transactions.AddRange(sales.Select(ii => new ProductTransactionItem
            {
                Date = ii.Invoice!.InvoiceDate, TransactionType = "Sales",
                ReferenceNumber = ii.Invoice.InvoiceNumber, PartyName = ii.Invoice.Customer?.Name,
                Quantity = ii.Quantity, UnitPrice = ii.UnitPrice, TotalAmount = ii.TotalPrice
            }));

            // Sales Return
            var sReturns = await _context.SalesReturnItems.Include(ri => ri.SalesReturn).ThenInclude(sr => sr!.Customer)
                .Where(ri => ri.ProductId == productId && ri.SalesReturn!.TenantId == tenantId)
                .Where(ri => !filter.FromDate.HasValue || ri.SalesReturn!.ReturnDate >= filter.FromDate)
                .Where(ri => !filter.ToDate.HasValue   || ri.SalesReturn!.ReturnDate <= filter.ToDate)
                .ToListAsync();
            transactions.AddRange(sReturns.Select(ri => new ProductTransactionItem
            {
                Date = ri.SalesReturn!.ReturnDate, TransactionType = "SalesReturn",
                ReferenceNumber = ri.SalesReturn.ReturnNumber, PartyName = ri.SalesReturn.Customer?.Name,
                Quantity = ri.Quantity, UnitPrice = ri.UnitPrice, TotalAmount = ri.TotalPrice
            }));

            // Purchase
            var purchases = await _context.PurchaseItems.Include(pi => pi.Purchase).ThenInclude(p => p!.Supplier)
                .Where(pi => pi.ProductId == productId && pi.Purchase!.TenantId == tenantId)
                .Where(pi => !filter.FromDate.HasValue || pi.Purchase!.PurchaseDate >= filter.FromDate)
                .Where(pi => !filter.ToDate.HasValue   || pi.Purchase!.PurchaseDate <= filter.ToDate)
                .ToListAsync();
            transactions.AddRange(purchases.Select(pi => new ProductTransactionItem
            {
                Date = pi.Purchase!.PurchaseDate, TransactionType = "Purchase",
                ReferenceNumber = pi.Purchase.PurchaseNumber, PartyName = pi.Purchase.Supplier?.Name,
                Quantity = pi.Quantity, UnitPrice = pi.UnitPrice, TotalAmount = pi.TotalPrice
            }));

            // Purchase Return
            var pReturns = await _context.PurchaseReturnItems.Include(ri => ri.PurchaseReturn).ThenInclude(pr => pr!.Supplier)
                .Where(ri => ri.ProductId == productId && ri.PurchaseReturn!.TenantId == tenantId)
                .Where(ri => !filter.FromDate.HasValue || ri.PurchaseReturn!.ReturnDate >= filter.FromDate)
                .Where(ri => !filter.ToDate.HasValue   || ri.PurchaseReturn!.ReturnDate <= filter.ToDate)
                .ToListAsync();
            transactions.AddRange(pReturns.Select(ri => new ProductTransactionItem
            {
                Date = ri.PurchaseReturn!.ReturnDate, TransactionType = "PurchaseReturn",
                ReferenceNumber = ri.PurchaseReturn.ReturnNumber, PartyName = ri.PurchaseReturn.Supplier?.Name,
                Quantity = ri.Quantity, UnitPrice = ri.UnitPrice, TotalAmount = ri.TotalPrice
            }));

            return new ProductMiniStatementResponse
            {
                ProductId = product.Id, ProductName = product.Name, SKU = product.SKU,
                CurrentStock = product.CurrentStock,
                Transactions = transactions.OrderByDescending(t => t.Date).ToList()
            };
        }

        // ── Product Stock Report ─────────────────────────────────────────────────
        public async Task<ProductStockReportResponse> GetProductStockReportAsync(int tenantId, ReportFilterRequest filter)
        {
            var q = _context.Products.Include(p => p.Category).Where(p => p.TenantId == tenantId);
            if (filter.CategoryId.HasValue) q = q.Where(p => p.CategoryId == filter.CategoryId);
            var products = await q.ToListAsync();
            return new ProductStockReportResponse
            {
                TotalProducts = products.Count,
                TotalStockValue = products.Sum(p => p.CurrentStock * p.PurchasePrice),
                Items = products.Select(p => new ProductStockReportItem
                {
                    ProductId = p.Id, ProductName = p.Name, SKU = p.SKU, Barcode = p.Barcode,
                    CategoryName = p.Category?.Name, CurrentStock = p.CurrentStock,
                    PurchasePrice = p.PurchasePrice, SellingPrice = p.SellingPrice,
                    StockValue = p.CurrentStock * p.PurchasePrice,
                    StockStatus = p.CurrentStock <= 0 ? "OutOfStock" : p.MinStockLevel.HasValue && p.CurrentStock <= p.MinStockLevel ? "LowStock" : "InStock",
                    ExpiryDate = p.ExpiryDate
                }).ToList()
            };
        }

        // ── Expired Stock Report ─────────────────────────────────────────────────
        public async Task<ExpiredStockReportResponse> GetExpiredStockReportAsync(int tenantId, int daysAhead = 30)
        {
            var today = DateTime.UtcNow.Date;
            var threshold = today.AddDays(daysAhead);
            var products = await _context.Products.Include(p => p.Category)
                .Where(p => p.TenantId == tenantId && p.ExpiryDate.HasValue && p.ExpiryDate <= threshold && p.CurrentStock > 0)
                .ToListAsync();
            return new ExpiredStockReportResponse
            {
                TotalExpiredProducts      = products.Count(p => p.ExpiryDate < today),
                TotalExpiringSoonProducts = products.Count(p => p.ExpiryDate >= today),
                TotalExpiredStockValue    = products.Where(p => p.ExpiryDate < today).Sum(p => p.CurrentStock * p.PurchasePrice),
                Items = products.Select(p =>
                {
                    var days = (int)(p.ExpiryDate!.Value.Date - today).TotalDays;
                    return new ExpiredStockReportItem
                    {
                        ProductId = p.Id, ProductName = p.Name, SKU = p.SKU,
                        CategoryName = p.Category?.Name, CurrentStock = p.CurrentStock,
                        PurchasePrice = p.PurchasePrice, StockValue = p.CurrentStock * p.PurchasePrice,
                        ExpiryDate = p.ExpiryDate,
                        ExpiryStatus = days < 0 ? "Expired" : days <= 7 ? "ExpiringSoon" : "Valid",
                        DaysToExpiry = days
                    };
                }).OrderBy(p => p.DaysToExpiry).ToList()
            };
        }
    }
}
