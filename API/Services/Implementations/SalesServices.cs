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
    // ==================== INVOICE SERVICE ====================
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;
        public InvoiceService(ApplicationDbContext context) { _context = context; }

        public async Task<(List<InvoiceResponse> Items, int TotalCount)> GetAllInvoicesAsync(int tenantId, PaginationRequest pagination)
        {
            var query = _context.Invoices.Include(i => i.Customer).Include(i => i.Items).Where(i => i.TenantId == tenantId);
            if (!string.IsNullOrEmpty(pagination.SearchQuery))
                query = query.Where(i => i.InvoiceNumber.Contains(pagination.SearchQuery) || (i.Customer != null && i.Customer.Name.Contains(pagination.SearchQuery)));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(i => i.CreatedAt).Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize)
                .Select(i => MapToResponseStatic(i)).ToListAsync();
            return (items, total);
        }

        public async Task<InvoiceResponse?> GetInvoiceByIdAsync(int id, int tenantId)
        {
            var i = await _context.Invoices.Include(i => i.Customer).Include(i => i.Items).ThenInclude(item => item.Product).Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId);
            return i == null ? null : MapToResponseStatic(i);
        }

        public async Task<InvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request, int tenantId)
        {
            var invoiceNumber = await GetNextInvoiceNumberAsync(tenantId);
            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber, CustomerId = request.CustomerId,
                InvoiceDate = request.InvoiceDate ?? DateTime.UtcNow, DueDate = request.DueDate,
                DiscountAmount = request.DiscountAmount, RoundOff = request.RoundOff,
                PaymentMethod = request.PaymentMethod, Notes = request.Notes,
                TermsAndConditions = request.TermsAndConditions, BranchId = request.BranchId,
                SalesPersonId = request.SalesPersonId, TenantId = tenantId
            };

            decimal subTotal = 0, taxTotal = 0;
            foreach (var item in request.Items)
            {
                var product = await _context.Products.Include(p => p.TaxRate).FirstOrDefaultAsync(p => p.Id == item.ProductId && p.TenantId == tenantId)
                    ?? throw new KeyNotFoundException($"Product {item.ProductId} not found");
                var taxRate = item.TaxRateId.HasValue ? await _context.TaxRates.FindAsync(item.TaxRateId) : product.TaxRate;
                var lineTotal = item.Quantity * item.UnitPrice;
                var discountAmt = item.DiscountType == DiscountType.Percentage ? lineTotal * (item.DiscountValue ?? 0) / 100 : (item.DiscountValue ?? 0);
                var taxableAmount = lineTotal - discountAmt;
                var taxAmount = taxRate != null ? taxableAmount * taxRate.Percentage / 100 : 0;

                invoice.Items.Add(new InvoiceItem
                {
                    ProductId = item.ProductId, ProductName = product.Name, HSNCode = product.HSNCode,
                    Quantity = item.Quantity, UnitPrice = item.UnitPrice, DiscountAmount = discountAmt,
                    DiscountType = item.DiscountType, DiscountValue = item.DiscountValue,
                    TaxAmount = taxAmount, TaxPercentage = taxRate?.Percentage,
                    CGSTAmount = taxRate?.CGSTPercentage.HasValue == true ? taxableAmount * taxRate.CGSTPercentage!.Value / 100 : null,
                    SGSTAmount = taxRate?.SGSTPercentage.HasValue == true ? taxableAmount * taxRate.SGSTPercentage!.Value / 100 : null,
                    IGSTAmount = taxRate?.IGSTPercentage.HasValue == true ? taxableAmount * taxRate.IGSTPercentage!.Value / 100 : null,
                    TotalPrice = taxableAmount + taxAmount, TaxRateId = taxRate?.Id, TenantId = tenantId
                });
                subTotal += lineTotal - discountAmt;
                taxTotal += taxAmount;

                // Update stock
                if (!product.IsService)
                {
                    var prevStock = product.CurrentStock;
                    product.CurrentStock -= item.Quantity;
                    _context.StockTransactions.Add(new StockTransaction { ProductId = product.Id, TransactionType = StockTransactionType.Sale, Quantity = -item.Quantity, PreviousStock = prevStock, NewStock = product.CurrentStock, ReferenceNumber = invoiceNumber, ReferenceType = "Invoice", Notes = "Sale", TenantId = tenantId });
                }
            }

            invoice.SubTotal = subTotal;
            invoice.TaxAmount = taxTotal;
            invoice.TotalAmount = subTotal + taxTotal - request.DiscountAmount + request.RoundOff;
            invoice.DueAmount = invoice.TotalAmount;
            invoice.Status = InvoiceStatus.Pending;

            // If payment method provided, mark as paid
            if (request.PaymentMethod.HasValue)
            {
                invoice.PaidAmount = invoice.TotalAmount;
                invoice.DueAmount = 0;
                invoice.Status = InvoiceStatus.Paid;
                invoice.Payments.Add(new Payment { Amount = invoice.TotalAmount, PaymentMethod = request.PaymentMethod.Value, PaymentDate = DateTime.UtcNow, TenantId = tenantId });
            }

            // Update customer stats
            if (request.CustomerId.HasValue)
            {
                var customer = await _context.Customers.FindAsync(request.CustomerId);
                if (customer != null)
                {
                    customer.TotalPurchaseAmount += invoice.TotalAmount;
                    customer.TotalPurchaseCount++;
                    customer.LastPurchaseDate = DateTime.UtcNow;
                    if (invoice.DueAmount > 0) customer.OutstandingBalance += invoice.DueAmount;
                }
            }

            // Increment invoice counter
            var settings = await _context.InvoiceSettings.FirstOrDefaultAsync(s => s.TenantId == tenantId);
            if (settings != null) settings.NextInvoiceNumber++;

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return (await GetInvoiceByIdAsync(invoice.Id, tenantId))!;
        }

        public async Task<InvoiceResponse> UpdateInvoiceAsync(int id, UpdateInvoiceRequest request, int tenantId)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId) ?? throw new KeyNotFoundException("Invoice not found");
            if (request.DueDate.HasValue) invoice.DueDate = request.DueDate;
            if (request.Notes != null) invoice.Notes = request.Notes;
            if (request.TermsAndConditions != null) invoice.TermsAndConditions = request.TermsAndConditions;
            if (request.Status.HasValue) invoice.Status = request.Status.Value;
            await _context.SaveChangesAsync();
            return (await GetInvoiceByIdAsync(id, tenantId))!;
        }

        public async Task<bool> DeleteInvoiceAsync(int id, int tenantId)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId) ?? throw new KeyNotFoundException("Invoice not found");
            invoice.IsDeleted = true; invoice.Status = InvoiceStatus.Cancelled;
            await _context.SaveChangesAsync(); return true;
        }

        public async Task<string> GetNextInvoiceNumberAsync(int tenantId)
        {
            var settings = await _context.InvoiceSettings.FirstOrDefaultAsync(s => s.TenantId == tenantId);
            var prefix = settings?.InvoicePrefix ?? "INV";
            var number = settings?.NextInvoiceNumber ?? 1;
            return $"{prefix}-{DateTime.UtcNow:yyyy}-{number:D5}";
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(int id, int tenantId)
        {
            // Placeholder — will be implemented via IInvoiceDocumentService
            var invoice = await GetInvoiceByIdAsync(id, tenantId) ?? throw new KeyNotFoundException("Invoice not found");
            return System.Text.Encoding.UTF8.GetBytes($"Invoice PDF for {invoice.InvoiceNumber}");
        }

        internal static InvoiceResponse MapToResponseStatic(Invoice i) => new()
        {
            Id = i.Id, InvoiceNumber = i.InvoiceNumber, CustomerName = i.Customer?.Name, CustomerPhone = i.Customer?.Phone, CustomerEmail = i.Customer?.Email,
            InvoiceDate = i.InvoiceDate, DueDate = i.DueDate, SubTotal = i.SubTotal, TaxAmount = i.TaxAmount, DiscountAmount = i.DiscountAmount,
            RoundOff = i.RoundOff, TotalAmount = i.TotalAmount, PaidAmount = i.PaidAmount, DueAmount = i.DueAmount, Status = i.Status.ToString(),
            PaymentMethodName = i.PaymentMethod?.ToString(), Notes = i.Notes, PdfUrl = i.PdfUrl,
            Items = i.Items.Select(item => new InvoiceItemResponse { Id = item.Id, ProductId = item.ProductId, ProductName = item.ProductName, HSNCode = item.HSNCode, Quantity = item.Quantity, UnitPrice = item.UnitPrice, DiscountAmount = item.DiscountAmount, TaxAmount = item.TaxAmount, TaxPercentage = item.TaxPercentage, TotalPrice = item.TotalPrice }).ToList(),
            Payments = i.Payments.Select(p => new PaymentResponse { Id = p.Id, Amount = p.Amount, PaymentMethod = p.PaymentMethod.ToString(), PaymentDate = p.PaymentDate, ReferenceNumber = p.ReferenceNumber, Status = p.Status.ToString() }).ToList()
        };
    }

    // ==================== POS SERVICE ====================
    public class POSService : IPOSService
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ApplicationDbContext _context;
        public POSService(IInvoiceService invoiceService, ApplicationDbContext context) { _invoiceService = invoiceService; _context = context; }

        public async Task<InvoiceResponse> CreatePOSBillAsync(POSBillRequest request, int tenantId)
        {
            var invoiceRequest = new CreateInvoiceRequest
            {
                CustomerId = request.CustomerId, PaymentMethod = request.PaymentMethod,
                DiscountAmount = request.DiscountAmount, BranchId = request.BranchId,
                Items = request.Items.Select(i => new CreateInvoiceItemRequest { ProductId = i.ProductId, Quantity = i.Quantity, UnitPrice = i.UnitPrice ?? 0 }).ToList()
            };

            // Fill in prices from product table if not provided
            foreach (var item in invoiceRequest.Items.Where(i => i.UnitPrice == 0))
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null) item.UnitPrice = product.SellingPrice;
            }

            return await _invoiceService.CreateInvoiceAsync(invoiceRequest, tenantId);
        }

        public async Task<InvoiceResponse> GetLastBillAsync(int tenantId)
        {
            var invoice = await _context.Invoices.Include(i => i.Customer).Include(i => i.Items).Include(i => i.Payments)
                .Where(i => i.TenantId == tenantId).OrderByDescending(i => i.CreatedAt).FirstOrDefaultAsync();
            if (invoice == null) throw new KeyNotFoundException("No invoices found");
            return InvoiceService.MapToResponseStatic(invoice);
        }
    }

    // ==================== PURCHASE SERVICE ====================
    public class PurchaseService : IPurchaseService
    {
        private readonly ApplicationDbContext _context;
        public PurchaseService(ApplicationDbContext context) { _context = context; }

        public async Task<(List<PurchaseResponse> Items, int TotalCount)> GetAllPurchasesAsync(int tenantId, PaginationRequest pagination)
        {
            var query = _context.Purchases.Include(p => p.Supplier).Where(p => p.TenantId == tenantId);
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(p => p.CreatedAt).Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize)
                .Select(p => new PurchaseResponse { Id = p.Id, PurchaseNumber = p.PurchaseNumber, SupplierName = p.Supplier != null ? p.Supplier.Name : null, PurchaseDate = p.PurchaseDate, DueDate = p.DueDate, SubTotal = p.SubTotal, TaxAmount = p.TaxAmount, DiscountAmount = p.DiscountAmount, TotalAmount = p.TotalAmount, PaidAmount = p.PaidAmount, DueAmount = p.DueAmount, Status = p.Status.ToString() }).ToListAsync();
            return (items, total);
        }

        public async Task<PurchaseResponse?> GetPurchaseByIdAsync(int id, int tenantId)
        {
            var p = await _context.Purchases.Include(p => p.Supplier).Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
            return p == null ? null : new PurchaseResponse { Id = p.Id, PurchaseNumber = p.PurchaseNumber, SupplierName = p.Supplier?.Name, PurchaseDate = p.PurchaseDate, DueDate = p.DueDate, SubTotal = p.SubTotal, TaxAmount = p.TaxAmount, DiscountAmount = p.DiscountAmount, TotalAmount = p.TotalAmount, PaidAmount = p.PaidAmount, DueAmount = p.DueAmount, Status = p.Status.ToString() };
        }

        public async Task<PurchaseResponse> CreatePurchaseAsync(CreatePurchaseRequest request, int tenantId)
        {
            var purchaseNumber = $"PUR-{DateTime.UtcNow:yyyy}-{(await _context.Purchases.CountAsync(p => p.TenantId == tenantId) + 1):D5}";
            var purchase = new Purchase { PurchaseNumber = purchaseNumber, SupplierId = request.SupplierId, PurchaseDate = request.PurchaseDate ?? DateTime.UtcNow, DueDate = request.DueDate, Notes = request.Notes, ReferenceNumber = request.ReferenceNumber, BranchId = request.BranchId, TenantId = tenantId };

            decimal subTotal = 0, taxTotal = 0;
            foreach (var item in request.Items)
            {
                var product = await _context.Products.Include(p => p.TaxRate).FirstOrDefaultAsync(p => p.Id == item.ProductId) ?? throw new KeyNotFoundException($"Product {item.ProductId} not found");
                var taxRate = item.TaxRateId.HasValue ? await _context.TaxRates.FindAsync(item.TaxRateId) : product.TaxRate;
                var lineTotal = item.Quantity * item.UnitPrice;
                var taxAmount = taxRate != null ? (lineTotal - item.DiscountAmount) * taxRate.Percentage / 100 : 0;
                purchase.Items.Add(new PurchaseItem { ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = item.UnitPrice, TaxAmount = taxAmount, DiscountAmount = item.DiscountAmount, TotalPrice = lineTotal - item.DiscountAmount + taxAmount, TaxRateId = taxRate?.Id, ExpiryDate = item.ExpiryDate, BatchNumber = item.BatchNumber, TenantId = tenantId });
                subTotal += lineTotal - item.DiscountAmount;
                taxTotal += taxAmount;

                // Update stock
                var prevStock = product.CurrentStock;
                product.CurrentStock += item.Quantity;
                _context.StockTransactions.Add(new StockTransaction { ProductId = product.Id, TransactionType = StockTransactionType.Purchase, Quantity = item.Quantity, PreviousStock = prevStock, NewStock = product.CurrentStock, ReferenceNumber = purchaseNumber, ReferenceType = "Purchase", TenantId = tenantId });
            }

            purchase.SubTotal = subTotal; purchase.TaxAmount = taxTotal; purchase.TotalAmount = subTotal + taxTotal; purchase.DueAmount = purchase.TotalAmount;
            purchase.Status = PurchaseStatus.Received;
            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();
            return (await GetPurchaseByIdAsync(purchase.Id, tenantId))!;
        }

        public async Task<PurchaseResponse> UpdatePurchaseAsync(int id, UpdatePurchaseRequest request, int tenantId)
        {
            var p = await _context.Purchases.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId) ?? throw new KeyNotFoundException("Purchase not found");
            if (request.DueDate.HasValue) p.DueDate = request.DueDate;
            if (request.Notes != null) p.Notes = request.Notes;
            if (request.ReferenceNumber != null) p.ReferenceNumber = request.ReferenceNumber;
            await _context.SaveChangesAsync();
            return (await GetPurchaseByIdAsync(id, tenantId))!;
        }

        public async Task<bool> DeletePurchaseAsync(int id, int tenantId)
        { var p = await _context.Purchases.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId) ?? throw new KeyNotFoundException("Purchase not found"); p.IsDeleted = true; p.Status = PurchaseStatus.Cancelled; await _context.SaveChangesAsync(); return true; }
    }
}
