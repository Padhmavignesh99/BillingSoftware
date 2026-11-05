using MasterBilling.API.Data;
using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Models.Entities;
using MasterBilling.API.Models.Enums;
using MasterBilling.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasterBilling.API.Services.Implementations
{
    // Stub implementations for remaining services

    public class CustomerGroupService : ICustomerGroupService { private readonly ApplicationDbContext _c; public CustomerGroupService(ApplicationDbContext c) { _c = c; } public async Task<List<CustomerGroup>> GetAllAsync(int t) => await _c.CustomerGroups.Where(g => g.TenantId == t).ToListAsync(); public async Task<CustomerGroup> CreateAsync(CreateCustomerGroupRequest r, int t) { var g = new CustomerGroup { Name = r.Name, Description = r.Description, DiscountPercentage = r.DiscountPercentage, TenantId = t }; _c.CustomerGroups.Add(g); await _c.SaveChangesAsync(); return g; } public async Task<CustomerGroup> UpdateAsync(int id, CreateCustomerGroupRequest r) { var g = await _c.CustomerGroups.FindAsync(id) ?? throw new KeyNotFoundException(); g.Name = r.Name; g.Description = r.Description; g.DiscountPercentage = r.DiscountPercentage; await _c.SaveChangesAsync(); return g; } public async Task<bool> DeleteAsync(int id) { var g = await _c.CustomerGroups.FindAsync(id) ?? throw new KeyNotFoundException(); g.IsDeleted = true; await _c.SaveChangesAsync(); return true; } }

    public class CustomerLedgerService : ICustomerLedgerService { private readonly ApplicationDbContext _c; public CustomerLedgerService(ApplicationDbContext c) { _c = c; } public async Task<List<CustomerLedger>> GetLedgerAsync(int cid, int t) => await _c.CustomerLedgers.Where(l => l.CustomerId == cid && l.TenantId == t).OrderByDescending(l => l.TransactionDate).ToListAsync(); public async Task<decimal> GetDueBalanceAsync(int cid, int t) { var customer = await _c.Customers.FindAsync(cid); return customer?.OutstandingBalance ?? 0; } public async Task<List<InvoiceResponse>> GetPurchaseHistoryAsync(int cid, int t) => await _c.Invoices.Where(i => i.CustomerId == cid && i.TenantId == t).OrderByDescending(i => i.InvoiceDate).Select(i => new InvoiceResponse { Id = i.Id, InvoiceNumber = i.InvoiceNumber, InvoiceDate = i.InvoiceDate, TotalAmount = i.TotalAmount, PaidAmount = i.PaidAmount, DueAmount = i.DueAmount, Status = i.Status.ToString() }).ToListAsync(); }

    public class CustomerPaymentService : ICustomerPaymentService
    {
        private readonly ApplicationDbContext _c;
        public CustomerPaymentService(ApplicationDbContext c) { _c = c; }

        public async Task<List<CustomerPayment>> GetPaymentsAsync(int customerId, int tenantId) =>
            await _c.CustomerPayments.Where(p => p.CustomerId == customerId && p.TenantId == tenantId)
                .OrderByDescending(p => p.PaymentDate).ToListAsync();

        public async Task<CustomerPayment> CreatePaymentAsync(CreateCustomerPaymentRequest request, int tenantId)
        {
            var customer = await _c.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId && c.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Customer not found");
            if (request.Amount <= 0 || request.Amount > customer.OutstandingBalance)
                throw new ArgumentException("Payment must be positive and no more than the outstanding balance.");
            var payment = new CustomerPayment
            {
                CustomerId = request.CustomerId, InvoiceId = request.InvoiceId,
                Amount = request.Amount, PaymentMethod = request.PaymentMethod,
                ReferenceNumber = request.ReferenceNumber, Notes = request.Notes, TenantId = tenantId
            };
            customer.OutstandingBalance -= request.Amount;
            _c.CustomerPayments.Add(payment);
            if (request.InvoiceId.HasValue)
            {
                var invoice = await _c.Invoices.FirstOrDefaultAsync(i =>
                    i.Id == request.InvoiceId && i.CustomerId == request.CustomerId && i.TenantId == tenantId)
                    ?? throw new KeyNotFoundException("Invoice not found");
                if (request.Amount > invoice.DueAmount) throw new ArgumentException("Payment exceeds invoice due.");
                invoice.PaidAmount += request.Amount;
                invoice.DueAmount -= request.Amount;
                invoice.Status = invoice.DueAmount == 0 ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
            }
            await _c.SaveChangesAsync();
            return payment;
        }
    }
    public class CustomerCreditService : ICustomerCreditService
    {
        private readonly ApplicationDbContext _c;
        public CustomerCreditService(ApplicationDbContext c) { _c = c; }

        public async Task<List<CustomerCredit>> GetCreditsAsync(int customerId, int tenantId)
            => await _c.CustomerCredits.Include(c => c.Customer).Include(c => c.Invoice)
                   .Where(c => c.CustomerId == customerId && c.TenantId == tenantId)
                   .OrderByDescending(c => c.CreatedAt).ToListAsync();

        /// <summary>Create a credit bill. userId tracks which billing user created it.</summary>
        public async Task<CustomerCredit> CreateCreditAsync(int customerId, int? invoiceId, decimal amount, int tenantId, int? userId = null, DateTime? dueDate = null, string? notes = null)
        {
            if (amount <= 0) throw new ArgumentException("Credit amount must be positive.");
            var customer = await _c.Customers.FirstOrDefaultAsync(c => c.Id == customerId && c.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Customer not found");
            if (invoiceId.HasValue && !await _c.Invoices.AnyAsync(i => i.Id == invoiceId && i.CustomerId == customerId && i.TenantId == tenantId))
                throw new ArgumentException("Invoice does not belong to this customer.");
            var credit = new CustomerCredit
            {
                CustomerId    = customerId,
                InvoiceId     = invoiceId,
                Amount        = amount,
                BalanceAmount = amount,
                PaidAmount    = 0,
                DueDate       = dueDate ?? DateTime.UtcNow.AddDays(30),
                Status        = "Pending",
                Notes         = notes,
                TenantId      = tenantId,
                UserId        = userId
            };
            _c.CustomerCredits.Add(credit);
            if (!invoiceId.HasValue) customer.OutstandingBalance += amount;
            await _c.SaveChangesAsync();
            return credit;
        }

        /// <summary>Get all credit bills created by a specific user (user-wise view).</summary>
        public async Task<List<CustomerCredit>> GetCreditsByUserAsync(int userId, int tenantId)
            => await _c.CustomerCredits.Include(c => c.Customer).Include(c => c.Invoice)
                   .Where(c => c.UserId == userId && c.TenantId == tenantId)
                   .OrderByDescending(c => c.CreatedAt).ToListAsync();

        /// <summary>Settle a credit bill partially or fully.</summary>
        public async Task<CustomerCredit> SettleCreditAsync(int creditId, decimal paymentAmount, int tenantId)
        {
            var credit = await _c.CustomerCredits.FirstOrDefaultAsync(c => c.Id == creditId && c.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"Credit bill {creditId} not found.");
            if (paymentAmount <= 0 || paymentAmount > credit.BalanceAmount)
                throw new ArgumentException("Payment must be positive and no more than the outstanding balance.");
            credit.PaidAmount    += paymentAmount;
            credit.BalanceAmount  = credit.Amount - credit.PaidAmount;
            credit.Status         = credit.BalanceAmount <= 0 ? "Paid" : "PartiallyPaid";
            var customer = await _c.Customers.FirstOrDefaultAsync(c => c.Id == credit.CustomerId && c.TenantId == tenantId);
            if (customer != null) customer.OutstandingBalance = Math.Max(0, customer.OutstandingBalance - paymentAmount);
            if (credit.InvoiceId.HasValue)
            {
                var invoice = await _c.Invoices.FirstOrDefaultAsync(i => i.Id == credit.InvoiceId && i.TenantId == tenantId);
                if (invoice != null)
                {
                    invoice.PaidAmount += paymentAmount;
                    invoice.DueAmount = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);
                    invoice.Status = invoice.DueAmount == 0 ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
                }
            }
            await _c.SaveChangesAsync();
            return credit;
        }

        /// <summary>Get total outstanding credit balance for a specific user across all their credit bills.</summary>
        public async Task<decimal> GetUserCreditBalanceAsync(int userId, int tenantId)
            => await _c.CustomerCredits
                   .Where(c => c.UserId == userId && c.TenantId == tenantId && c.Status != "Paid")
                   .SumAsync(c => c.BalanceAmount);
    }

    public class SupplierGroupService : ISupplierGroupService { private readonly ApplicationDbContext _c; public SupplierGroupService(ApplicationDbContext c) { _c = c; } public async Task<List<SupplierGroup>> GetAllAsync(int t) => await _c.SupplierGroups.Where(g => g.TenantId == t).ToListAsync(); public async Task<SupplierGroup> CreateAsync(CreateSupplierGroupRequest r, int t) { var g = new SupplierGroup { Name = r.Name, Description = r.Description, TenantId = t }; _c.SupplierGroups.Add(g); await _c.SaveChangesAsync(); return g; } public async Task<SupplierGroup> UpdateAsync(int id, CreateSupplierGroupRequest r) { var g = await _c.SupplierGroups.FindAsync(id) ?? throw new KeyNotFoundException(); g.Name = r.Name; g.Description = r.Description; await _c.SaveChangesAsync(); return g; } public async Task<bool> DeleteAsync(int id) { var g = await _c.SupplierGroups.FindAsync(id) ?? throw new KeyNotFoundException(); g.IsDeleted = true; await _c.SaveChangesAsync(); return true; } }

    public class SupplierLedgerService : ISupplierLedgerService { private readonly ApplicationDbContext _c; public SupplierLedgerService(ApplicationDbContext c) { _c = c; } public async Task<List<SupplierLedger>> GetLedgerAsync(int sid, int t) => await _c.SupplierLedgers.Where(l => l.SupplierId == sid && l.TenantId == t).OrderByDescending(l => l.TransactionDate).ToListAsync(); public async Task<decimal> GetBalanceAsync(int sid, int t) { var s = await _c.Suppliers.FindAsync(sid); return s?.OutstandingBalance ?? 0; } }

    public class SupplierPaymentService : ISupplierPaymentService { private readonly ApplicationDbContext _c; public SupplierPaymentService(ApplicationDbContext c) { _c = c; } public async Task<List<SupplierPayment>> GetPaymentsAsync(int sid, int t) => await _c.SupplierPayments.Where(p => p.SupplierId == sid && p.TenantId == t).ToListAsync(); public async Task<SupplierPayment> CreatePaymentAsync(CreateSupplierPaymentRequest r, int t) { var p = new SupplierPayment { SupplierId = r.SupplierId, PurchaseId = r.PurchaseId, Amount = r.Amount, PaymentMethod = r.PaymentMethod, ReferenceNumber = r.ReferenceNumber, Notes = r.Notes, TenantId = t }; _c.SupplierPayments.Add(p); await _c.SaveChangesAsync(); return p; } }

    public class SupplierCreditService : ISupplierCreditService { private readonly ApplicationDbContext _c; public SupplierCreditService(ApplicationDbContext c) { _c = c; } public async Task<List<SupplierCredit>> GetCreditsAsync(int sid, int t) => await _c.SupplierCredits.Where(c => c.SupplierId == sid && c.TenantId == t).ToListAsync(); }

    public class PaymentMethodService : IPaymentMethodService { private readonly ApplicationDbContext _c; public PaymentMethodService(ApplicationDbContext c) { _c = c; } public async Task<List<PaymentMethodEntity>> GetAllAsync(int t) => await _c.PaymentMethods.Where(m => m.TenantId == t).ToListAsync(); public async Task<PaymentMethodEntity> CreateAsync(CreatePaymentMethodRequest r, int t) { var m = new PaymentMethodEntity { Name = r.Name, Description = r.Description, IsDefault = r.IsDefault, Icon = r.Icon, SortOrder = r.SortOrder, TenantId = t }; _c.PaymentMethods.Add(m); await _c.SaveChangesAsync(); return m; } public async Task<PaymentMethodEntity> UpdateAsync(int id, CreatePaymentMethodRequest r) { var m = await _c.PaymentMethods.FindAsync(id) ?? throw new KeyNotFoundException(); m.Name = r.Name; m.Description = r.Description; m.IsDefault = r.IsDefault; await _c.SaveChangesAsync(); return m; } public async Task<bool> DeleteAsync(int id) { var m = await _c.PaymentMethods.FindAsync(id) ?? throw new KeyNotFoundException(); m.IsDeleted = true; await _c.SaveChangesAsync(); return true; } }

    public class RefundService : IRefundService { private readonly ApplicationDbContext _c; public RefundService(ApplicationDbContext c) { _c = c; } public async Task<List<Refund>> GetAllRefundsAsync(int t) => await _c.Refunds.Where(r => r.TenantId == t).OrderByDescending(r => r.RefundDate).ToListAsync(); public async Task<Refund?> GetRefundByIdAsync(int id) => await _c.Refunds.FindAsync(id); public async Task<Refund> CreateRefundAsync(CreateRefundRequest r, int t) { var refund = new Refund { InvoiceId = r.InvoiceId, PaymentId = r.PaymentId, CustomerId = r.CustomerId, Amount = r.Amount, Reason = r.Reason, RefundMethod = r.RefundMethod, Notes = r.Notes, TenantId = t }; _c.Refunds.Add(refund); await _c.SaveChangesAsync(); return refund; } public async Task<Refund> UpdateRefundStatusAsync(int id, string status) { var r = await _c.Refunds.FindAsync(id) ?? throw new KeyNotFoundException(); r.Status = Enum.Parse<RefundStatus>(status, true); await _c.SaveChangesAsync(); return r; } }

    public class CashRegisterService : ICashRegisterService { private readonly ApplicationDbContext _c; public CashRegisterService(ApplicationDbContext c) { _c = c; } public async Task<CashRegister> OpenRegisterAsync(OpenCashRegisterRequest r, int t, int uid) { var reg = new CashRegister { RegisterName = r.RegisterName, OpeningBalance = r.OpeningBalance, BranchId = r.BranchId, OpenedByUserId = uid, TenantId = t }; _c.CashRegisters.Add(reg); await _c.SaveChangesAsync(); return reg; } public async Task<CashRegister> CloseRegisterAsync(CloseCashRegisterRequest r, int uid) { var reg = await _c.CashRegisters.FindAsync(r.RegisterId) ?? throw new KeyNotFoundException(); reg.ClosingBalance = r.ClosingBalance; reg.ClosedByUserId = uid; reg.ClosedAt = DateTime.UtcNow; reg.Status = CashRegisterStatus.Closed; reg.Notes = r.Notes; await _c.SaveChangesAsync(); return reg; } public async Task<CashRegister?> GetCurrentRegisterAsync(int t, int uid) => await _c.CashRegisters.FirstOrDefaultAsync(r => r.TenantId == t && r.OpenedByUserId == uid && r.Status == CashRegisterStatus.Open); public async Task<List<CashRegister>> GetRegisterHistoryAsync(int t) => await _c.CashRegisters.Where(r => r.TenantId == t).OrderByDescending(r => r.OpenedAt).ToListAsync(); }

    public class PaymentGatewayService : IPaymentGatewayService { private readonly ApplicationDbContext _c; public PaymentGatewayService(ApplicationDbContext c) { _c = c; } public async Task<PaymentGatewayTransaction> CreatePaymentLinkAsync(int iid, int t) { var tx = new PaymentGatewayTransaction { InvoiceId = iid, GatewayName = "Razorpay", OrderId = Guid.NewGuid().ToString("N")[..10], TenantId = t }; _c.PaymentGatewayTransactions.Add(tx); await _c.SaveChangesAsync(); return tx; } public async Task<bool> HandleWebhookAsync(string payload) => true; public async Task<PaymentGatewayTransaction?> GetTransactionStatusAsync(string txId) => await _c.PaymentGatewayTransactions.FirstOrDefaultAsync(t => t.GatewayTransactionId == txId); }

    public class ExpenseCategoryService : IExpenseCategoryService { private readonly ApplicationDbContext _c; public ExpenseCategoryService(ApplicationDbContext c) { _c = c; } public async Task<List<ExpenseCategory>> GetAllAsync(int t) => await _c.ExpenseCategories.Where(c => c.TenantId == t && !c.IsDeleted).ToListAsync(); public async Task<ExpenseCategory> CreateAsync(CreateExpenseCategoryRequest r, int t) { var c = new ExpenseCategory { Name = r.Name, Description = r.Description, Icon = r.Icon, ParentCategoryId = r.ParentCategoryId, TenantId = t }; _c.ExpenseCategories.Add(c); await _c.SaveChangesAsync(); return c; } public async Task<ExpenseCategory> UpdateAsync(int id, CreateExpenseCategoryRequest r) { var c = await _c.ExpenseCategories.FindAsync(id) ?? throw new KeyNotFoundException(); c.Name = r.Name; c.Description = r.Description; c.Icon = r.Icon; await _c.SaveChangesAsync(); return c; } public async Task<bool> DeleteAsync(int id) { var c = await _c.ExpenseCategories.FindAsync(id) ?? throw new KeyNotFoundException(); c.IsDeleted = true; await _c.SaveChangesAsync(); return true; } }

    public class ExpensePaymentService : IExpensePaymentService { private readonly ApplicationDbContext _c; public ExpensePaymentService(ApplicationDbContext c) { _c = c; } public async Task<List<ExpensePayment>> GetPaymentsByExpenseAsync(int eid) => await _c.ExpensePayments.Where(p => p.ExpenseId == eid).ToListAsync(); public async Task<ExpensePayment> CreatePaymentAsync(CreateExpensePaymentRequest r, int t) { var p = new ExpensePayment { ExpenseId = r.ExpenseId, Amount = r.Amount, PaymentMethod = r.PaymentMethod, ReferenceNumber = r.ReferenceNumber, Notes = r.Notes, TenantId = t }; _c.ExpensePayments.Add(p); await _c.SaveChangesAsync(); return p; } }

    public class PurchaseOrderService : IPurchaseOrderService { private readonly ApplicationDbContext _c; public PurchaseOrderService(ApplicationDbContext c) { _c = c; } public async Task<List<PurchaseOrder>> GetAllAsync(int t) => await _c.PurchaseOrders.Include(po => po.Supplier).Where(po => po.TenantId == t).OrderByDescending(po => po.OrderDate).ToListAsync(); public async Task<PurchaseOrder?> GetByIdAsync(int id) => await _c.PurchaseOrders.Include(po => po.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(po => po.Id == id); public async Task<PurchaseOrder> CreateAsync(CreatePurchaseOrderRequest r, int t) { var po = new PurchaseOrder { OrderNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}", SupplierId = r.SupplierId, ExpectedDeliveryDate = r.ExpectedDeliveryDate, Notes = r.Notes, BranchId = r.BranchId, TenantId = t }; foreach (var i in r.Items) { po.Items.Add(new PurchaseOrderItem { ProductId = i.ProductId, Quantity = i.Quantity, UnitPrice = i.UnitPrice, TotalPrice = i.Quantity * i.UnitPrice, TenantId = t }); po.TotalAmount += i.Quantity * i.UnitPrice; } _c.PurchaseOrders.Add(po); await _c.SaveChangesAsync(); return po; } public async Task<PurchaseOrder> UpdateAsync(int id, UpdatePurchaseOrderRequest r) { var po = await _c.PurchaseOrders.FindAsync(id) ?? throw new KeyNotFoundException(); if (r.ExpectedDeliveryDate.HasValue) po.ExpectedDeliveryDate = r.ExpectedDeliveryDate; if (r.Notes != null) po.Notes = r.Notes; if (r.Status.HasValue) po.Status = r.Status.Value; await _c.SaveChangesAsync(); return po; } public async Task<bool> DeleteAsync(int id) { var po = await _c.PurchaseOrders.FindAsync(id) ?? throw new KeyNotFoundException(); po.IsDeleted = true; await _c.SaveChangesAsync(); return true; } }

    public class PurchasePaymentService : IPurchasePaymentService { private readonly ApplicationDbContext _c; public PurchasePaymentService(ApplicationDbContext c) { _c = c; } public async Task<List<PurchasePayment>> GetPaymentsByPurchaseAsync(int pid) => await _c.PurchasePayments.Where(p => p.PurchaseId == pid).ToListAsync(); public async Task<PurchasePayment> CreatePaymentAsync(CreatePurchasePaymentRequest r, int t) { var p = new PurchasePayment { PurchaseId = r.PurchaseId, Amount = r.Amount, PaymentMethod = r.PaymentMethod, ReferenceNumber = r.ReferenceNumber, Notes = r.Notes, TenantId = t }; _c.PurchasePayments.Add(p); var purchase = await _c.Purchases.FindAsync(r.PurchaseId); if (purchase != null) { purchase.PaidAmount += r.Amount; purchase.DueAmount = purchase.TotalAmount - purchase.PaidAmount; } await _c.SaveChangesAsync(); return p; } }

    public class PurchaseReturnService : IPurchaseReturnService
    {
        private readonly ApplicationDbContext _c;
        public PurchaseReturnService(ApplicationDbContext c) { _c = c; }

        public async Task<List<PurchaseReturn>> GetAllAsync(int tenantId) =>
            await _c.PurchaseReturns.Include(ret => ret.Items)
                .Where(ret => ret.TenantId == tenantId).OrderByDescending(ret => ret.ReturnDate).ToListAsync();

        public async Task<PurchaseReturn?> GetByIdAsync(int id) =>
            await _c.PurchaseReturns.Include(ret => ret.Items).FirstOrDefaultAsync(ret => ret.Id == id);

        public async Task<PurchaseReturn> CreateAsync(CreatePurchaseReturnRequest request, int tenantId)
        {
            var purchase = await _c.Purchases.Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == request.PurchaseId && p.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Purchase not found");
            if (request.Items.Count == 0) throw new ArgumentException("Select at least one return item.");

            var ret = new PurchaseReturn
            {
                ReturnNumber = $"PR-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                PurchaseId = purchase.Id,
                SupplierId = purchase.SupplierId,
                Reason = request.Reason,
                Notes = request.Notes,
                Status = "Completed",
                TenantId = tenantId
            };
            foreach (var item in request.Items)
            {
                var purchased = purchase.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (purchased == null || item.Quantity <= 0 || item.Quantity > purchased.Quantity)
                    throw new ArgumentException("Invalid purchase return quantity.");
                var previouslyReturned = await _c.PurchaseReturnItems
                    .Where(i => i.PurchaseReturn != null && i.PurchaseReturn.PurchaseId == purchase.Id && i.ProductId == item.ProductId)
                    .SumAsync(i => i.Quantity);
                if (previouslyReturned + item.Quantity > purchased.Quantity)
                    throw new ArgumentException("Return quantity exceeds the purchased quantity.");
                var product = await _c.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId && p.TenantId == tenantId)
                    ?? throw new KeyNotFoundException("Product not found");
                if (product.CurrentStock < item.Quantity)
                    throw new InvalidOperationException("Insufficient stock for purchase return.");
                var previousStock = product.CurrentStock;
                product.CurrentStock -= item.Quantity;
                _c.StockTransactions.Add(new StockTransaction
                {
                    ProductId = product.Id, TransactionType = StockTransactionType.Return,
                    Quantity = -item.Quantity, PreviousStock = previousStock, NewStock = product.CurrentStock,
                    ReferenceNumber = ret.ReturnNumber, ReferenceType = "PurchaseReturn", TenantId = tenantId
                });
                ret.Items.Add(new PurchaseReturnItem
                {
                    ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = purchased.UnitPrice,
                    TotalPrice = item.Quantity * purchased.UnitPrice, TenantId = tenantId
                });
                ret.TotalAmount += item.Quantity * purchased.UnitPrice;
            }
            _c.PurchaseReturns.Add(ret);
            await _c.SaveChangesAsync();
            return ret;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ret = await _c.PurchaseReturns.Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException("Return not found");
            foreach (var item in ret.Items)
            {
                var product = await _c.Products.FindAsync(item.ProductId)
                    ?? throw new KeyNotFoundException("Product not found");
                var previousStock = product.CurrentStock;
                product.CurrentStock += item.Quantity;
                _c.StockTransactions.Add(new StockTransaction
                {
                    ProductId = product.Id, TransactionType = StockTransactionType.Return,
                    Quantity = item.Quantity, PreviousStock = previousStock, NewStock = product.CurrentStock,
                    ReferenceNumber = ret.ReturnNumber, ReferenceType = "PurchaseReturnReversal", TenantId = ret.TenantId
                });
            }
            ret.IsDeleted = true;
            await _c.SaveChangesAsync();
            return true;
        }
    }
    public class GoodsReceiptService : IGoodsReceiptService { private readonly ApplicationDbContext _c; public GoodsReceiptService(ApplicationDbContext c) { _c = c; } public async Task<List<GoodsReceipt>> GetAllAsync(int t) => await _c.GoodsReceipts.Include(g => g.Supplier).Where(g => g.TenantId == t).ToListAsync(); public async Task<GoodsReceipt?> GetByIdAsync(int id) => await _c.GoodsReceipts.Include(g => g.Items).FirstOrDefaultAsync(g => g.Id == id); public async Task<GoodsReceipt> CreateAsync(CreateGoodsReceiptRequest r, int t) { var gr = new GoodsReceipt { ReceiptNumber = $"GRN-{DateTime.UtcNow:yyyyMMddHHmmss}", PurchaseOrderId = r.PurchaseOrderId, PurchaseId = r.PurchaseId, SupplierId = r.SupplierId, Notes = r.Notes, TenantId = t }; foreach (var i in r.Items) gr.Items.Add(new GoodsReceiptItem { ProductId = i.ProductId, OrderedQuantity = i.OrderedQuantity, ReceivedQuantity = i.ReceivedQuantity, DamagedQuantity = i.DamagedQuantity, Notes = i.Notes, TenantId = t }); _c.GoodsReceipts.Add(gr); await _c.SaveChangesAsync(); return gr; } }

    public class SalesReturnService : ISalesReturnService
    {
        private readonly ApplicationDbContext _c;
        public SalesReturnService(ApplicationDbContext c) { _c = c; }

        public async Task<List<SalesReturn>> GetAllAsync(int tenantId) =>
            await _c.SalesReturns.Include(ret => ret.Items)
                .Where(ret => ret.TenantId == tenantId).OrderByDescending(ret => ret.ReturnDate).ToListAsync();

        public async Task<SalesReturn?> GetByIdAsync(int id) =>
            await _c.SalesReturns.Include(ret => ret.Items).FirstOrDefaultAsync(ret => ret.Id == id);

        public async Task<SalesReturn> CreateAsync(CreateSalesReturnRequest request, int tenantId)
        {
            var invoice = await _c.Invoices.Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId && i.TenantId == tenantId)
                ?? throw new KeyNotFoundException("Invoice not found");
            if (request.Items.Count == 0) throw new ArgumentException("Select at least one return item.");

            var ret = new SalesReturn
            {
                ReturnNumber = $"SR-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                InvoiceId = invoice.Id,
                CustomerId = request.CustomerId ?? invoice.CustomerId,
                Reason = request.Reason,
                Notes = request.Notes,
                Status = "Completed",
                TenantId = tenantId
            };
            foreach (var item in request.Items)
            {
                var sold = invoice.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (sold == null || item.Quantity <= 0 || item.Quantity > sold.Quantity)
                    throw new ArgumentException("Invalid sales return quantity.");
                var previouslyReturned = await _c.SalesReturnItems
                    .Where(i => i.SalesReturn != null && i.SalesReturn.InvoiceId == invoice.Id && i.ProductId == item.ProductId)
                    .SumAsync(i => i.Quantity);
                if (previouslyReturned + item.Quantity > sold.Quantity)
                    throw new ArgumentException("Return quantity exceeds the sold quantity.");
                var product = await _c.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId && p.TenantId == tenantId)
                    ?? throw new KeyNotFoundException("Product not found");
                var previousStock = product.CurrentStock;
                product.CurrentStock += item.Quantity;
                _c.StockTransactions.Add(new StockTransaction
                {
                    ProductId = product.Id, TransactionType = StockTransactionType.Return,
                    Quantity = item.Quantity, PreviousStock = previousStock, NewStock = product.CurrentStock,
                    ReferenceNumber = ret.ReturnNumber, ReferenceType = "SalesReturn", TenantId = tenantId
                });
                ret.Items.Add(new SalesReturnItem
                {
                    ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = sold.UnitPrice,
                    TotalPrice = item.Quantity * sold.UnitPrice, TenantId = tenantId
                });
                ret.TotalAmount += item.Quantity * sold.UnitPrice;
            }
            _c.SalesReturns.Add(ret);
            await _c.SaveChangesAsync();
            return ret;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ret = await _c.SalesReturns.Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException("Return not found");
            foreach (var item in ret.Items)
            {
                var product = await _c.Products.FindAsync(item.ProductId)
                    ?? throw new KeyNotFoundException("Product not found");
                if (product.CurrentStock < item.Quantity)
                    throw new InvalidOperationException("Cannot delete return because the restocked items have already been used.");
                var previousStock = product.CurrentStock;
                product.CurrentStock -= item.Quantity;
                _c.StockTransactions.Add(new StockTransaction
                {
                    ProductId = product.Id, TransactionType = StockTransactionType.Return,
                    Quantity = -item.Quantity, PreviousStock = previousStock, NewStock = product.CurrentStock,
                    ReferenceNumber = ret.ReturnNumber, ReferenceType = "SalesReturnReversal", TenantId = ret.TenantId
                });
            }
            ret.IsDeleted = true;
            await _c.SaveChangesAsync();
            return true;
        }
    }
    public class QuotationService : IQuotationService { private readonly ApplicationDbContext _c; private readonly IInvoiceService _inv; public QuotationService(ApplicationDbContext c, IInvoiceService inv) { _c = c; _inv = inv; } public async Task<List<Quotation>> GetAllAsync(int t) => await _c.Quotations.Include(q => q.Customer).Where(q => q.TenantId == t).ToListAsync(); public async Task<Quotation?> GetByIdAsync(int id) => await _c.Quotations.Include(q => q.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(q => q.Id == id); public async Task<Quotation> CreateAsync(CreateQuotationRequest r, int t) { var q = new Quotation { QuotationNumber = $"QT-{DateTime.UtcNow:yyyyMMddHHmmss}", CustomerId = r.CustomerId, ValidUntil = r.ValidUntil, Notes = r.Notes, TenantId = t }; foreach (var i in r.Items) { q.Items.Add(new QuotationItem { ProductId = i.ProductId, Quantity = i.Quantity, UnitPrice = i.UnitPrice, DiscountAmount = i.DiscountAmount, TenantId = t }); q.SubTotal += i.Quantity * i.UnitPrice; } q.TotalAmount = q.SubTotal; _c.Quotations.Add(q); await _c.SaveChangesAsync(); return q; } public async Task<Quotation> UpdateAsync(int id, UpdateQuotationRequest r) { var q = await _c.Quotations.FindAsync(id) ?? throw new KeyNotFoundException(); if (r.ValidUntil.HasValue) q.ValidUntil = r.ValidUntil; if (r.Notes != null) q.Notes = r.Notes; if (r.Status.HasValue) q.Status = r.Status.Value; await _c.SaveChangesAsync(); return q; } public async Task<bool> DeleteAsync(int id) { var q = await _c.Quotations.FindAsync(id) ?? throw new KeyNotFoundException(); q.IsDeleted = true; await _c.SaveChangesAsync(); return true; } public async Task<InvoiceResponse> ConvertToInvoiceAsync(int qid, int t) { var q = await _c.Quotations.Include(q => q.Items).FirstOrDefaultAsync(q => q.Id == qid) ?? throw new KeyNotFoundException(); var req = new CreateInvoiceRequest { CustomerId = q.CustomerId, Items = q.Items.Select(i => new CreateInvoiceItemRequest { ProductId = i.ProductId, Quantity = i.Quantity, UnitPrice = i.UnitPrice }).ToList() }; var invoice = await _inv.CreateInvoiceAsync(req, t); q.Status = QuotationStatus.ConvertedToInvoice; q.ConvertedInvoiceId = invoice.Id; await _c.SaveChangesAsync(); return invoice; } }

    public class SalesOrderService : ISalesOrderService { private readonly ApplicationDbContext _c; private readonly IInvoiceService _inv; public SalesOrderService(ApplicationDbContext c, IInvoiceService inv) { _c = c; _inv = inv; } public async Task<List<SalesOrder>> GetAllAsync(int t) => await _c.SalesOrders.Include(so => so.Customer).Where(so => so.TenantId == t).ToListAsync(); public async Task<SalesOrder?> GetByIdAsync(int id) => await _c.SalesOrders.Include(so => so.Items).FirstOrDefaultAsync(so => so.Id == id); public async Task<SalesOrder> CreateAsync(CreateSalesOrderRequest r, int t) { var so = new SalesOrder { OrderNumber = $"SO-{DateTime.UtcNow:yyyyMMddHHmmss}", CustomerId = r.CustomerId, DeliveryDate = r.DeliveryDate, Notes = r.Notes, ShippingAddress = r.ShippingAddress, TenantId = t }; foreach (var i in r.Items) { so.Items.Add(new SalesOrderItem { ProductId = i.ProductId, Quantity = i.Quantity, UnitPrice = i.UnitPrice, DiscountAmount = i.DiscountAmount, TenantId = t }); so.SubTotal += i.Quantity * i.UnitPrice; } so.TotalAmount = so.SubTotal; _c.SalesOrders.Add(so); await _c.SaveChangesAsync(); return so; } public async Task<SalesOrder> UpdateAsync(int id, UpdateSalesOrderRequest r) { var so = await _c.SalesOrders.FindAsync(id) ?? throw new KeyNotFoundException(); if (r.DeliveryDate.HasValue) so.DeliveryDate = r.DeliveryDate; if (r.Notes != null) so.Notes = r.Notes; if (r.Status.HasValue) so.Status = r.Status.Value; await _c.SaveChangesAsync(); return so; } public async Task<bool> DeleteAsync(int id) { var so = await _c.SalesOrders.FindAsync(id) ?? throw new KeyNotFoundException(); so.IsDeleted = true; await _c.SaveChangesAsync(); return true; } public async Task<InvoiceResponse> ConvertToInvoiceAsync(int soid, int t) { var so = await _c.SalesOrders.Include(so => so.Items).FirstOrDefaultAsync(so => so.Id == soid) ?? throw new KeyNotFoundException(); var req = new CreateInvoiceRequest { CustomerId = so.CustomerId, Items = so.Items.Select(i => new CreateInvoiceItemRequest { ProductId = i.ProductId, Quantity = i.Quantity, UnitPrice = i.UnitPrice }).ToList() }; var inv = await _inv.CreateInvoiceAsync(req, t); so.Status = SalesOrderStatus.Delivered; so.ConvertedInvoiceId = inv.Id; await _c.SaveChangesAsync(); return inv; } }

    public class SubscriptionService : ISubscriptionService { private readonly ApplicationDbContext _c; public SubscriptionService(ApplicationDbContext c) { _c = c; } public async Task<List<SubscriptionPlanResponse>> GetPlansAsync() => await _c.SubscriptionPlans.Where(p => p.IsActive).Select(p => new SubscriptionPlanResponse { Id = p.Id, Name = p.Name, Description = p.Description, MonthlyPrice = p.MonthlyPrice, YearlyPrice = p.YearlyPrice, MaxProducts = p.MaxProducts, MaxUsers = p.MaxUsers, MaxBranches = p.MaxBranches, MaxInvoicesPerMonth = p.MaxInvoicesPerMonth, HasWhatsAppIntegration = p.HasWhatsAppIntegration, HasEmailIntegration = p.HasEmailIntegration, HasReports = p.HasReports, IsPopular = p.IsPopular }).ToListAsync(); public async Task<SubscriptionResponse?> GetCurrentSubscriptionAsync(int bid) { var s = await _c.Subscriptions.Include(s => s.Plan).FirstOrDefaultAsync(s => s.BusinessId == bid && s.Status == SubscriptionStatus.Active); return s == null ? null : new SubscriptionResponse { Id = s.Id, PlanName = s.Plan?.Name ?? "", StartDate = s.StartDate, EndDate = s.EndDate, Status = s.Status.ToString(), BillingCycle = s.BillingCycle, AmountPaid = s.AmountPaid, AutoRenew = s.AutoRenew, DaysRemaining = (s.EndDate - DateTime.UtcNow).Days }; } public async Task<SubscriptionResponse> SubscribeAsync(SubscribeRequest r, int bid) { var plan = await _c.SubscriptionPlans.FindAsync(r.PlanId) ?? throw new KeyNotFoundException("Plan not found"); var sub = new Subscription { BusinessId = bid, PlanId = r.PlanId, StartDate = DateTime.UtcNow, EndDate = r.BillingCycle == "Yearly" ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddMonths(1), Status = SubscriptionStatus.Active, BillingCycle = r.BillingCycle, AmountPaid = r.BillingCycle == "Yearly" ? plan.YearlyPrice ?? plan.MonthlyPrice * 12 : plan.MonthlyPrice }; _c.Subscriptions.Add(sub); await _c.SaveChangesAsync(); return new SubscriptionResponse { Id = sub.Id, PlanName = plan.Name, StartDate = sub.StartDate, EndDate = sub.EndDate, Status = sub.Status.ToString(), BillingCycle = sub.BillingCycle, AmountPaid = sub.AmountPaid, AutoRenew = sub.AutoRenew, DaysRemaining = (sub.EndDate - DateTime.UtcNow).Days }; } public async Task<bool> CancelSubscriptionAsync(int bid, CancelSubscriptionRequest r) { var s = await _c.Subscriptions.FirstOrDefaultAsync(s => s.BusinessId == bid && s.Status == SubscriptionStatus.Active); if (s == null) return false; s.Status = SubscriptionStatus.Cancelled; s.CancelledAt = DateTime.UtcNow; s.CancellationReason = r.Reason; await _c.SaveChangesAsync(); return true; } public async Task<bool> HandleWebhookAsync(string p) => true; }

    public class SubscriptionPlanService : ISubscriptionPlanService { private readonly ApplicationDbContext _c; public SubscriptionPlanService(ApplicationDbContext c) { _c = c; } public async Task<List<SubscriptionPlanResponse>> GetAllPlansAsync() => await _c.SubscriptionPlans.Select(p => new SubscriptionPlanResponse { Id = p.Id, Name = p.Name, Description = p.Description, MonthlyPrice = p.MonthlyPrice, YearlyPrice = p.YearlyPrice, MaxProducts = p.MaxProducts, MaxUsers = p.MaxUsers, MaxBranches = p.MaxBranches, IsPopular = p.IsPopular }).ToListAsync(); public async Task<SubscriptionPlanResponse?> GetPlanByIdAsync(int id) { var p = await _c.SubscriptionPlans.FindAsync(id); return p == null ? null : new SubscriptionPlanResponse { Id = p.Id, Name = p.Name, MonthlyPrice = p.MonthlyPrice }; } public async Task<SubscriptionPlanResponse> CreatePlanAsync(CreateSubscriptionPlanRequest r) { var p = new SubscriptionPlan { Name = r.Name, Description = r.Description, MonthlyPrice = r.MonthlyPrice, YearlyPrice = r.YearlyPrice, MaxProducts = r.MaxProducts, MaxUsers = r.MaxUsers, MaxBranches = r.MaxBranches, MaxInvoicesPerMonth = r.MaxInvoicesPerMonth, HasWhatsAppIntegration = r.HasWhatsAppIntegration, HasEmailIntegration = r.HasEmailIntegration, HasReports = r.HasReports, HasMultiCurrency = r.HasMultiCurrency, HasInventoryManagement = r.HasInventoryManagement }; _c.SubscriptionPlans.Add(p); await _c.SaveChangesAsync(); return new SubscriptionPlanResponse { Id = p.Id, Name = p.Name, MonthlyPrice = p.MonthlyPrice }; } public async Task<SubscriptionPlanResponse> UpdatePlanAsync(int id, CreateSubscriptionPlanRequest r) { var p = await _c.SubscriptionPlans.FindAsync(id) ?? throw new KeyNotFoundException(); p.Name = r.Name; p.MonthlyPrice = r.MonthlyPrice; p.YearlyPrice = r.YearlyPrice; await _c.SaveChangesAsync(); return new SubscriptionPlanResponse { Id = p.Id, Name = p.Name, MonthlyPrice = p.MonthlyPrice }; } public async Task<bool> DeletePlanAsync(int id) { var p = await _c.SubscriptionPlans.FindAsync(id) ?? throw new KeyNotFoundException(); p.IsActive = false; await _c.SaveChangesAsync(); return true; } }

    public class SubscriptionPaymentService : ISubscriptionPaymentService { private readonly ApplicationDbContext _c; public SubscriptionPaymentService(ApplicationDbContext c) { _c = c; } public async Task<List<SubscriptionPayment>> GetPaymentsAsync(int bid) => await _c.SubscriptionPayments.Where(p => p.BusinessId == bid).OrderByDescending(p => p.PaymentDate).ToListAsync(); }

    public class TrialService : ITrialService { private readonly ApplicationDbContext _c; public TrialService(ApplicationDbContext c) { _c = c; } public async Task<Trial?> GetActiveTrialAsync(int bid) => await _c.Trials.FirstOrDefaultAsync(t => t.BusinessId == bid && t.EndDate > DateTime.UtcNow && !t.IsConverted); public async Task<Trial> StartTrialAsync(int bid) { var trial = new Trial { BusinessId = bid, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(14) }; _c.Trials.Add(trial); await _c.SaveChangesAsync(); return trial; } public async Task<bool> ConvertTrialAsync(int bid, int pid) { var trial = await GetActiveTrialAsync(bid); if (trial != null) { trial.IsConverted = true; await _c.SaveChangesAsync(); } return true; } }

    public class PlanFeatureService : IPlanFeatureService { private readonly ApplicationDbContext _c; public PlanFeatureService(ApplicationDbContext c) { _c = c; } public async Task<List<PlanFeature>> GetFeaturesAsync(int pid) => await _c.PlanFeatures.Where(f => f.PlanId == pid).ToListAsync(); public async Task<PlanFeature> AddFeatureAsync(int pid, string name, string? desc) { var f = new PlanFeature { PlanId = pid, FeatureName = name, Description = desc }; _c.PlanFeatures.Add(f); await _c.SaveChangesAsync(); return f; } public async Task<bool> RemoveFeatureAsync(int fid) { var f = await _c.PlanFeatures.FindAsync(fid) ?? throw new KeyNotFoundException(); _c.PlanFeatures.Remove(f); await _c.SaveChangesAsync(); return true; } }

    public class FileService : IFileService { private readonly ApplicationDbContext _c; public FileService(ApplicationDbContext c) { _c = c; } public async Task<FileResponse> UploadFileAsync(Stream stream, string fileName, string contentType, int tenantId, string? refType = null, int? refId = null) { var uniqueName = $"{Guid.NewGuid():N}_{fileName}"; var filePath = Path.Combine("uploads", uniqueName); Directory.CreateDirectory("uploads"); using (var fs = new FileStream(filePath, FileMode.Create)) await stream.CopyToAsync(fs); var file = new FileUpload { FileName = uniqueName, OriginalFileName = fileName, FilePath = filePath, FileUrl = $"/uploads/{uniqueName}", ContentType = contentType, FileSize = new FileInfo(filePath).Length, FileType = contentType.StartsWith("image") ? FileType.Image : contentType == "application/pdf" ? FileType.PDF : FileType.Document, ReferenceType = refType, ReferenceId = refId, TenantId = tenantId }; _c.FileUploads.Add(file); await _c.SaveChangesAsync(); return new FileResponse { Id = file.Id, FileName = file.FileName, OriginalFileName = file.OriginalFileName, FileUrl = file.FileUrl, ContentType = file.ContentType, FileSize = file.FileSize, CreatedAt = file.CreatedAt }; } public async Task<FileUpload?> GetFileByIdAsync(int id) => await _c.FileUploads.FindAsync(id); public async Task<bool> DeleteFileAsync(int id) { var f = await _c.FileUploads.FindAsync(id) ?? throw new KeyNotFoundException(); if (File.Exists(f.FilePath)) File.Delete(f.FilePath); _c.FileUploads.Remove(f); await _c.SaveChangesAsync(); return true; } }

    public class InvoiceDocumentService : IInvoiceDocumentService { private readonly ApplicationDbContext _c; public InvoiceDocumentService(ApplicationDbContext c) { _c = c; } public async Task<byte[]> GeneratePdfAsync(int iid, int t) { var invoice = await _c.Invoices.Include(i => i.Items).Include(i => i.Customer).FirstOrDefaultAsync(i => i.Id == iid && i.TenantId == t) ?? throw new KeyNotFoundException("Invoice not found"); return System.Text.Encoding.UTF8.GetBytes($"PDF for {invoice.InvoiceNumber}"); } public async Task<string> GenerateAndSavePdfAsync(int iid, int t) { var pdf = await GeneratePdfAsync(iid, t); var path = Path.Combine("uploads", "invoices", $"INV-{iid}.pdf"); Directory.CreateDirectory(Path.GetDirectoryName(path)!); await File.WriteAllBytesAsync(path, pdf); return path; } public async Task<byte[]> GetPdfAsync(int iid, int t) => await GeneratePdfAsync(iid, t); public async Task<string> GetPreviewHtmlAsync(int iid, int t) { var invoice = await _c.Invoices.Include(i => i.Items).Include(i => i.Customer).FirstOrDefaultAsync(i => i.Id == iid && i.TenantId == t); return $"<html><body><h1>{invoice?.InvoiceNumber}</h1></body></html>"; } }

    public class AuditLogService : IAuditLogService { private readonly ApplicationDbContext _c; public AuditLogService(ApplicationDbContext c) { _c = c; } public async Task<List<AuditLog>> GetLogsAsync(int t, PaginationRequest p) => await _c.AuditLogs.Where(a => a.TenantId == t).OrderByDescending(a => a.Timestamp).Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToListAsync(); public async Task LogAsync(int? t, int? uid, string action, string entityType, int? entityId, string? old, string? @new) { _c.AuditLogs.Add(new AuditLog { TenantId = t, UserId = uid, Action = action, EntityType = entityType, EntityId = entityId, OldValues = old, NewValues = @new }); await _c.SaveChangesAsync(); } }

    public class BackupService : IBackupService { private readonly ApplicationDbContext _c; public BackupService(ApplicationDbContext c) { _c = c; } public async Task<BackupRecord> CreateBackupAsync(int t) { var b = new BackupRecord { TenantId = t, FileName = $"backup-{t}-{DateTime.UtcNow:yyyyMMddHHmmss}.bak", FilePath = "backups/", BackupDate = DateTime.UtcNow }; _c.BackupRecords.Add(b); await _c.SaveChangesAsync(); return b; } public async Task<List<BackupRecord>> GetBackupsAsync(int t) => await _c.BackupRecords.Where(b => b.TenantId == t).OrderByDescending(b => b.BackupDate).ToListAsync(); public async Task<byte[]> DownloadBackupAsync(int id) => System.Text.Encoding.UTF8.GetBytes("Backup file placeholder"); }

    public class LookupService : ILookupService { private readonly ApplicationDbContext _c; public LookupService(ApplicationDbContext c) { _c = c; } public async Task<List<Lookup>> GetLookupsByCategoryAsync(string cat) => await _c.Lookups.Where(l => l.Category == cat && l.IsActive).OrderBy(l => l.SortOrder).ToListAsync(); public async Task<Lookup> CreateLookupAsync(string cat, string key, string val, string? desc) { var l = new Lookup { Category = cat, Key = key, Value = val, Description = desc }; _c.Lookups.Add(l); await _c.SaveChangesAsync(); return l; } }




    // ==================== PRINT SETTINGS SERVICE ====================
    public class PrintSettingsService : IPrintSettingsService
    {
        private readonly ApplicationDbContext _c;
        public PrintSettingsService(ApplicationDbContext c) { _c = c; }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static PrintSettings BuildDefaults(int tenantId, int? branchId = null, int? counterId = null) => new()
        {
            TenantId             = tenantId,
            BranchId             = branchId,
            CounterId            = counterId,
            PaperSize            = "A4",
            Orientation          = "Portrait",
            MarginTopMm          = 10, MarginBottomMm = 10,
            MarginLeftMm         = 10, MarginRightMm  = 10,
            FontFamily           = "Arial",
            FontSizePt           = 10,
            HeaderFontSizePt     = 14,
            FooterFontSizePt     = 8,
            ShowBusinessLogo     = true,
            ShowBusinessAddress  = true,
            ShowGSTDetails       = true,
            ShowCustomerAddress  = true,
            ShowItemHSNCode      = true,
            ShowItemDiscount     = true,
            ShowTaxBreakdown     = true,
            ShowRoundOff         = true,
            ShowAmountInWords    = true,
            ShowTermsAndConditions = true,
            ShowFooterNote       = true,
            ShowPaymentDetails   = true,
            ShowDueAmount        = true,
            InvoiceCopies        = 1,
            QuotationCopies      = 1,
            PurchaseCopies       = 1,
            ReceiptCopies        = 1,
            InvoiceCopyLabel1    = "Original",
            InvoiceCopyLabel2    = "Customer Copy",
            InvoiceCopyLabel3    = "Office Copy",
            ThermalPrintEnabled  = false,
            ThermalPaperWidth    = "80mm",
            ThermalShowLogo      = true,
            ThermalShowQRCode    = true,
            ThermalShowCustomerName = true,
            ThermalAutocut       = true,
            ThermalLineFeedCount = 3,
            LabelPrintEnabled    = false,
            LabelPaperSize       = "50x25mm",
            LabelShowBarcode     = true,
            LabelShowMRP         = true,
            LabelShowProductName = true,
            LabelShowBusiness    = true,
            LabelsPerRow         = 2,
            ShowReportHeader     = true,
            ShowReportFooter     = true,
            ShowPageNumbers      = true,
            ShowPrintDateTime    = true,
            ShowReportLogo       = true,
            PrintPrimaryColor    = "#1a73e8",
            PrintInColor         = true,
            PrinterConnection    = "USB",
            OpenCashDrawer       = false,
            FooterText           = "Thank you! Visit again"
        };

        private static string BuildScopeLabel(PrintSettings s)
        {
            if (s.BranchId == null)  return "Tenant Default";
            if (s.CounterId == null) return $"Branch #{s.BranchId}";
            return $"Counter {s.CounterId} — Branch #{s.BranchId}";
        }

        private static PrintSettingsResponse MapToResponse(PrintSettings s) => new()
        {
            Id                    = s.Id,
            PaperSize             = s.PaperSize,
            CustomPaperWidthMm    = s.CustomPaperWidthMm,
            CustomPaperHeightMm   = s.CustomPaperHeightMm,
            Orientation           = s.Orientation,
            MarginTopMm           = s.MarginTopMm,
            MarginBottomMm        = s.MarginBottomMm,
            MarginLeftMm          = s.MarginLeftMm,
            MarginRightMm         = s.MarginRightMm,
            FontFamily            = s.FontFamily,
            FontSizePt            = s.FontSizePt,
            HeaderFontSizePt      = s.HeaderFontSizePt,
            FooterFontSizePt      = s.FooterFontSizePt,
            ShowBusinessLogo      = s.ShowBusinessLogo,
            ShowBusinessAddress   = s.ShowBusinessAddress,
            ShowGSTDetails        = s.ShowGSTDetails,
            ShowCustomerAddress   = s.ShowCustomerAddress,
            ShowItemHSNCode       = s.ShowItemHSNCode,
            ShowItemDiscount      = s.ShowItemDiscount,
            ShowTaxBreakdown      = s.ShowTaxBreakdown,
            ShowRoundOff          = s.ShowRoundOff,
            ShowAmountInWords     = s.ShowAmountInWords,
            ShowTermsAndConditions= s.ShowTermsAndConditions,
            ShowFooterNote        = s.ShowFooterNote,
            ShowSignatureLine     = s.ShowSignatureLine,
            ShowQRCode            = s.ShowQRCode,
            ShowBarcode           = s.ShowBarcode,
            ShowPaymentDetails    = s.ShowPaymentDetails,
            ShowDueAmount         = s.ShowDueAmount,
            ShowBankDetails       = s.ShowBankDetails,
            BankDetails           = s.BankDetails,
            UpiId                 = s.UpiId,
            InvoiceCopies         = s.InvoiceCopies,
            QuotationCopies       = s.QuotationCopies,
            PurchaseCopies        = s.PurchaseCopies,
            ReceiptCopies         = s.ReceiptCopies,
            InvoiceCopyLabel1     = s.InvoiceCopyLabel1,
            InvoiceCopyLabel2     = s.InvoiceCopyLabel2,
            InvoiceCopyLabel3     = s.InvoiceCopyLabel3,
            ThermalPrintEnabled   = s.ThermalPrintEnabled,
            ThermalPaperWidth     = s.ThermalPaperWidth,
            ThermalShowLogo       = s.ThermalShowLogo,
            ThermalShowQRCode     = s.ThermalShowQRCode,
            ThermalShowItemCode   = s.ThermalShowItemCode,
            ThermalShowCustomerName = s.ThermalShowCustomerName,
            ThermalAutocut        = s.ThermalAutocut,
            ThermalLineFeedCount  = s.ThermalLineFeedCount,
            LabelPrintEnabled     = s.LabelPrintEnabled,
            LabelPaperSize        = s.LabelPaperSize,
            LabelShowBarcode      = s.LabelShowBarcode,
            LabelShowMRP          = s.LabelShowMRP,
            LabelShowProductName  = s.LabelShowProductName,
            LabelShowBusiness     = s.LabelShowBusiness,
            LabelsPerRow          = s.LabelsPerRow,
            ShowReportHeader      = s.ShowReportHeader,
            ShowReportFooter      = s.ShowReportFooter,
            ShowPageNumbers       = s.ShowPageNumbers,
            ShowPrintDateTime     = s.ShowPrintDateTime,
            ShowReportLogo        = s.ShowReportLogo,
            PrintPrimaryColor     = s.PrintPrimaryColor,
            PrintInColor          = s.PrintInColor,
            DefaultPrinterName    = s.DefaultPrinterName,
            DefaultThermalPrinter = s.DefaultThermalPrinter,
            DefaultLabelPrinter   = s.DefaultLabelPrinter,
            PrinterIpAddress      = s.PrinterIpAddress,
            PrinterPort           = s.PrinterPort,
            PrinterConnection     = s.PrinterConnection,
            OpenCashDrawer        = s.OpenCashDrawer,
            FooterText            = s.FooterText,
            BranchId              = s.BranchId,
            CounterId             = s.CounterId,
            ScopeLabel            = BuildScopeLabel(s),
            UpdatedAt             = s.UpdatedAt
        };

        /// <summary>Applies non-null fields from the request onto the entity (shared by Update and Patch).</summary>
        private static void ApplyRequest(PrintSettings s, UpdatePrintSettingsRequest r)
        {
            if (r.PaperSize             != null) s.PaperSize             = r.PaperSize;
            if (r.CustomPaperWidthMm.HasValue)   s.CustomPaperWidthMm   = r.CustomPaperWidthMm;
            if (r.CustomPaperHeightMm.HasValue)  s.CustomPaperHeightMm  = r.CustomPaperHeightMm;
            if (r.Orientation           != null) s.Orientation           = r.Orientation;
            if (r.MarginTopMm.HasValue)    s.MarginTopMm    = r.MarginTopMm.Value;
            if (r.MarginBottomMm.HasValue) s.MarginBottomMm = r.MarginBottomMm.Value;
            if (r.MarginLeftMm.HasValue)   s.MarginLeftMm   = r.MarginLeftMm.Value;
            if (r.MarginRightMm.HasValue)  s.MarginRightMm  = r.MarginRightMm.Value;
            if (r.FontFamily            != null) s.FontFamily            = r.FontFamily;
            if (r.FontSizePt.HasValue)       s.FontSizePt       = r.FontSizePt.Value;
            if (r.HeaderFontSizePt.HasValue) s.HeaderFontSizePt = r.HeaderFontSizePt.Value;
            if (r.FooterFontSizePt.HasValue) s.FooterFontSizePt = r.FooterFontSizePt.Value;
            if (r.ShowBusinessLogo.HasValue)       s.ShowBusinessLogo       = r.ShowBusinessLogo.Value;
            if (r.ShowBusinessAddress.HasValue)    s.ShowBusinessAddress    = r.ShowBusinessAddress.Value;
            if (r.ShowGSTDetails.HasValue)         s.ShowGSTDetails         = r.ShowGSTDetails.Value;
            if (r.ShowCustomerAddress.HasValue)    s.ShowCustomerAddress    = r.ShowCustomerAddress.Value;
            if (r.ShowItemHSNCode.HasValue)        s.ShowItemHSNCode        = r.ShowItemHSNCode.Value;
            if (r.ShowItemDiscount.HasValue)       s.ShowItemDiscount       = r.ShowItemDiscount.Value;
            if (r.ShowTaxBreakdown.HasValue)       s.ShowTaxBreakdown       = r.ShowTaxBreakdown.Value;
            if (r.ShowRoundOff.HasValue)           s.ShowRoundOff           = r.ShowRoundOff.Value;
            if (r.ShowAmountInWords.HasValue)      s.ShowAmountInWords      = r.ShowAmountInWords.Value;
            if (r.ShowTermsAndConditions.HasValue) s.ShowTermsAndConditions = r.ShowTermsAndConditions.Value;
            if (r.ShowFooterNote.HasValue)         s.ShowFooterNote         = r.ShowFooterNote.Value;
            if (r.ShowSignatureLine.HasValue)      s.ShowSignatureLine      = r.ShowSignatureLine.Value;
            if (r.ShowQRCode.HasValue)             s.ShowQRCode             = r.ShowQRCode.Value;
            if (r.ShowBarcode.HasValue)            s.ShowBarcode            = r.ShowBarcode.Value;
            if (r.ShowPaymentDetails.HasValue)     s.ShowPaymentDetails     = r.ShowPaymentDetails.Value;
            if (r.ShowDueAmount.HasValue)          s.ShowDueAmount          = r.ShowDueAmount.Value;
            if (r.ShowBankDetails.HasValue)        s.ShowBankDetails        = r.ShowBankDetails.Value;
            if (r.BankDetails           != null) s.BankDetails           = r.BankDetails;
            if (r.UpiId                 != null) s.UpiId                 = r.UpiId;
            if (r.InvoiceCopies.HasValue)   s.InvoiceCopies   = r.InvoiceCopies.Value;
            if (r.QuotationCopies.HasValue) s.QuotationCopies = r.QuotationCopies.Value;
            if (r.PurchaseCopies.HasValue)  s.PurchaseCopies  = r.PurchaseCopies.Value;
            if (r.ReceiptCopies.HasValue)   s.ReceiptCopies   = r.ReceiptCopies.Value;
            if (r.InvoiceCopyLabel1     != null) s.InvoiceCopyLabel1     = r.InvoiceCopyLabel1;
            if (r.InvoiceCopyLabel2     != null) s.InvoiceCopyLabel2     = r.InvoiceCopyLabel2;
            if (r.InvoiceCopyLabel3     != null) s.InvoiceCopyLabel3     = r.InvoiceCopyLabel3;
            if (r.ThermalPrintEnabled.HasValue)     s.ThermalPrintEnabled     = r.ThermalPrintEnabled.Value;
            if (r.ThermalPaperWidth     != null)    s.ThermalPaperWidth       = r.ThermalPaperWidth;
            if (r.ThermalShowLogo.HasValue)         s.ThermalShowLogo         = r.ThermalShowLogo.Value;
            if (r.ThermalShowQRCode.HasValue)       s.ThermalShowQRCode       = r.ThermalShowQRCode.Value;
            if (r.ThermalShowItemCode.HasValue)     s.ThermalShowItemCode     = r.ThermalShowItemCode.Value;
            if (r.ThermalShowCustomerName.HasValue) s.ThermalShowCustomerName = r.ThermalShowCustomerName.Value;
            if (r.ThermalAutocut.HasValue)          s.ThermalAutocut          = r.ThermalAutocut.Value;
            if (r.ThermalLineFeedCount.HasValue)    s.ThermalLineFeedCount    = r.ThermalLineFeedCount.Value;
            if (r.LabelPrintEnabled.HasValue)   s.LabelPrintEnabled   = r.LabelPrintEnabled.Value;
            if (r.LabelPaperSize        != null) s.LabelPaperSize       = r.LabelPaperSize;
            if (r.LabelShowBarcode.HasValue)    s.LabelShowBarcode    = r.LabelShowBarcode.Value;
            if (r.LabelShowMRP.HasValue)        s.LabelShowMRP        = r.LabelShowMRP.Value;
            if (r.LabelShowProductName.HasValue)s.LabelShowProductName = r.LabelShowProductName.Value;
            if (r.LabelShowBusiness.HasValue)   s.LabelShowBusiness   = r.LabelShowBusiness.Value;
            if (r.LabelsPerRow.HasValue)        s.LabelsPerRow        = r.LabelsPerRow.Value;
            if (r.ShowReportHeader.HasValue)  s.ShowReportHeader  = r.ShowReportHeader.Value;
            if (r.ShowReportFooter.HasValue)  s.ShowReportFooter  = r.ShowReportFooter.Value;
            if (r.ShowPageNumbers.HasValue)   s.ShowPageNumbers   = r.ShowPageNumbers.Value;
            if (r.ShowPrintDateTime.HasValue) s.ShowPrintDateTime = r.ShowPrintDateTime.Value;
            if (r.ShowReportLogo.HasValue)    s.ShowReportLogo    = r.ShowReportLogo.Value;
            if (r.PrintPrimaryColor     != null) s.PrintPrimaryColor     = r.PrintPrimaryColor;
            if (r.PrintInColor.HasValue)         s.PrintInColor          = r.PrintInColor.Value;
            if (r.DefaultPrinterName    != null) s.DefaultPrinterName    = r.DefaultPrinterName;
            if (r.DefaultThermalPrinter != null) s.DefaultThermalPrinter = r.DefaultThermalPrinter;
            if (r.DefaultLabelPrinter   != null) s.DefaultLabelPrinter   = r.DefaultLabelPrinter;
            // Network / connection fields
            if (r.PrinterIpAddress      != null) s.PrinterIpAddress      = r.PrinterIpAddress;
            if (r.PrinterPort.HasValue)           s.PrinterPort           = r.PrinterPort;
            if (r.PrinterConnection     != null) s.PrinterConnection     = r.PrinterConnection;
            if (r.OpenCashDrawer.HasValue)        s.OpenCashDrawer        = r.OpenCashDrawer.Value;
            if (r.FooterText            != null) s.FooterText            = r.FooterText;
        }


        // ── Settings Hierarchy Resolution ─────────────────────────────────────────

        /// <summary>
        /// Resolves settings using the three-level hierarchy:
        /// counter-level → branch-level → tenant-level.
        /// Returns in-memory defaults if nothing is stored yet.
        /// </summary>
        private async Task<PrintSettings> ResolveSettingsAsync(int tenantId, int? branchId, int? counterId)
        {
            // 1. Counter-level (most specific)
            if (branchId.HasValue && counterId.HasValue)
            {
                var counter = await _c.PrintSettings
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId
                                           && x.BranchId == branchId
                                           && x.CounterId == counterId);
                if (counter != null) return counter;
            }

            // 2. Branch-level
            if (branchId.HasValue)
            {
                var branch = await _c.PrintSettings
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId
                                           && x.BranchId == branchId
                                           && x.CounterId == null);
                if (branch != null) return branch;
            }

            // 3. Tenant-level (fallback)
            var tenant = await _c.PrintSettings
                .FirstOrDefaultAsync(x => x.TenantId == tenantId
                                       && x.BranchId == null
                                       && x.CounterId == null);
            return tenant ?? BuildDefaults(tenantId);
        }

        // ── Core CRUD ─────────────────────────────────────────────────────────────

        public async Task<PrintSettingsResponse> GetPrintSettingsAsync(int tenantId, int? branchId = null, int? counterId = null)
            => MapToResponse(await ResolveSettingsAsync(tenantId, branchId, counterId));

        public async Task<PrintSettingsResponse> UpdatePrintSettingsAsync(int tenantId, UpdatePrintSettingsRequest request, int? branchId = null, int? counterId = null)
        {
            var effectiveBranchId  = branchId  ?? request.BranchId;
            var effectiveCounterId = counterId ?? request.CounterId;

            var s = await _c.PrintSettings.FirstOrDefaultAsync(x =>
                x.TenantId  == tenantId &&
                x.BranchId  == effectiveBranchId &&
                x.CounterId == effectiveCounterId);

            if (s == null)
            {
                s = BuildDefaults(tenantId, effectiveBranchId, effectiveCounterId);
                _c.PrintSettings.Add(s);
            }

            ApplyRequest(s, request);
            s.UpdatedAt = DateTime.UtcNow;
            await _c.SaveChangesAsync();
            return MapToResponse(s);
        }

        public async Task<PrintSettingsResponse> PatchPrintSettingsAsync(int tenantId, PatchPrintSettingsRequest request, int? branchId = null, int? counterId = null)
            => await UpdatePrintSettingsAsync(tenantId, request, branchId, counterId);

        public async Task<PrintSettingsResponse> ResetToDefaultsAsync(int tenantId, int? branchId = null, int? counterId = null)
        {
            var existing = await _c.PrintSettings.FirstOrDefaultAsync(x =>
                x.TenantId  == tenantId &&
                x.BranchId  == branchId &&
                x.CounterId == counterId);

            if (existing != null) _c.PrintSettings.Remove(existing);
            var defaults = BuildDefaults(tenantId, branchId, counterId);
            _c.PrintSettings.Add(defaults);
            await _c.SaveChangesAsync();
            return MapToResponse(defaults);
        }

        public async Task<List<PrintSettingsResponse>> GetAllScopedSettingsAsync(int tenantId)
        {
            var all = await _c.PrintSettings
                .Where(x => x.TenantId == tenantId)
                .OrderBy(x => x.BranchId)
                .ThenBy(x => x.CounterId)
                .ToListAsync();
            return all.Select(MapToResponse).ToList();
        }

        // ── Preview ───────────────────────────────────────────────────────────────

        public async Task<string> GetPrintPreviewHtmlAsync(int tenantId, string documentType, int? branchId = null, int? counterId = null)
        {
            var s       = await ResolveSettingsAsync(tenantId, branchId, counterId);
            var biz     = await _c.Businesses.FirstOrDefaultAsync(b => b.Id == tenantId);
            var bizName = biz?.Name ?? "Your Business Name";
            var gst     = biz?.GSTNumber ?? "GST1234567890";
            var color   = s.PrintPrimaryColor ?? "#1a73e8";
            var font    = s.FontFamily;
            var bodyPx  = (int)(s.FontSizePt * 1.33m);
            var hdrPx   = (int)(s.HeaderFontSizePt * 1.33m);
            var ftrPx   = (int)(s.FooterFontSizePt * 1.33m);
            var mt = s.MarginTopMm; var mb = s.MarginBottomMm;
            var ml = s.MarginLeftMm; var mr = s.MarginRightMm;
            var docLabel = documentType.ToUpperInvariant() switch
            {
                "QUOTATION" => "Quotation",
                "RECEIPT"   => "Receipt",
                "REPORT"    => "Report",
                _           => "Invoice"
            };
            var pageWidth = s.PaperSize switch
            {
                "A5"          => "148mm",
                "Thermal80mm" => "80mm",
                "Thermal58mm" => "58mm",
                "Letter"      => "216mm",
                "Legal"       => "216mm",
                _             => "210mm"
            };

            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"">
  <title>Print Preview — {docLabel}</title>
  <style>
    *{{ box-sizing:border-box; margin:0; padding:0; }}
    body{{ font-family:'{font}',Arial,sans-serif; font-size:{bodyPx}px; color:#333; background:#f0f2f5; }}
    .page{{ background:#fff; width:{pageWidth}; margin:20px auto;
            padding:{mt}mm {mr}mm {mb}mm {ml}mm; box-shadow:0 4px 24px rgba(0,0,0,.12); position:relative; }}
    .header{{ display:flex; justify-content:space-between; align-items:flex-start; border-bottom:2px solid {color}; padding-bottom:12px; margin-bottom:16px; }}
    .biz-name{{ font-size:{hdrPx}px; font-weight:700; color:{color}; }}
    .biz-info{{ font-size:{ftrPx + 1}px; color:#555; margin-top:4px; }}
    .badge{{ background:{color}; color:#fff; padding:6px 18px; border-radius:4px; font-size:{hdrPx - 2}px; font-weight:700; text-align:right; }}
    .badge small{{ display:block; font-size:{ftrPx}px; font-weight:400; opacity:.9; }}
    table{{ width:100%; border-collapse:collapse; margin-top:12px; font-size:{bodyPx}px; }}
    th{{ background:{color}; color:#fff; padding:7px 10px; text-align:left; }}
    td{{ padding:6px 10px; border-bottom:1px solid #e5e7eb; }}
    tr:nth-child(even) td{{ background:#f9fafb; }}
    .totals{{ margin-top:12px; display:flex; justify-content:flex-end; }}
    .totals-box{{ width:240px; }}
    .totals-row{{ display:flex; justify-content:space-between; padding:4px 0; font-size:{bodyPx}px; }}
    .totals-row.grand{{ font-weight:700; color:{color}; border-top:2px solid {color}; padding-top:6px; font-size:{bodyPx + 2}px; }}
    .footer-note{{ margin-top:18px; font-size:{ftrPx}px; color:#777; border-top:1px dashed #ddd; padding-top:10px; }}
    .watermark{{ position:absolute; top:50%; left:50%; transform:translate(-50%,-50%) rotate(-30deg);
                 font-size:72px; color:rgba(0,0,0,.04); font-weight:900; pointer-events:none; white-space:nowrap; }}
    .scope-badge{{ font-size:10px; background:#f3f4f6; border:1px solid #e5e7eb; color:#6b7280; padding:2px 8px; border-radius:9999px; display:inline-block; margin-bottom:8px; }}
  </style>
</head>
<body>
<div style=""text-align:center;margin-bottom:4px""><span class=""scope-badge"">Preview — {BuildScopeLabel(s)}</span></div>
<div class=""page"">
  <div class=""watermark"">PREVIEW</div>
  <div class=""header"">
    <div>
      {(s.ShowBusinessLogo ? $"<div style='font-size:{hdrPx + 6}px;font-weight:900;color:{color};letter-spacing:-1px;margin-bottom:4px'>{bizName[0]}</div>" : "")}
      <div class=""biz-name"">{bizName}</div>
      {(s.ShowBusinessAddress ? $"<div class=\"biz-info\">123 Business Street, Chennai – 600001<br>Ph: +91 98765 43210 | {biz?.Email ?? "info@yourbiz.com"}</div>" : "")}
      {(s.ShowGSTDetails ? $"<div class=\"biz-info\" style='margin-top:4px'>GSTIN: {gst}</div>" : "")}
    </div>
    <div class=""badge"">
      {docLabel.ToUpper()}
      <small>#INV-2024-00001</small>
      <small style='margin-top:4px'>Date: {DateTime.Now:dd-MMM-yyyy}</small>
    </div>
  </div>

  {(s.ShowCustomerAddress ? $@"<div style='display:flex;justify-content:space-between;margin-bottom:14px'>
    <div><strong>Bill To:</strong><br>Sample Customer<br>456 Customer Lane, Mumbai – 400001<br>Ph: +91 90000 12345</div>
    <div style='text-align:right;font-size:{ftrPx + 1}px;color:#555'>
      Due Date: {DateTime.Now.AddDays(30):dd-MMM-yyyy}<br>
      {(s.ShowDueAmount ? "Balance Due: <strong>&#8377; 1,180.00</strong>" : "")}
    </div>
  </div>" : "")}

  <table>
    <thead>
      <tr>
        <th>#</th><th>Item</th>
        {(s.ShowItemHSNCode ? "<th>HSN</th>" : "")}
        <th>Qty</th><th>Rate</th>
        {(s.ShowItemDiscount ? "<th>Disc.</th>" : "")}
        {(s.ShowTaxBreakdown ? "<th>CGST</th><th>SGST</th>" : "<th>Tax</th>")}
        <th>Amount</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>1</td><td>Sample Product A</td>
        {(s.ShowItemHSNCode ? "<td>8471</td>" : "")}
        <td>2</td><td>&#8377;500.00</td>
        {(s.ShowItemDiscount ? "<td>&#8377;0.00</td>" : "")}
        {(s.ShowTaxBreakdown ? "<td>&#8377;45.00</td><td>&#8377;45.00</td>" : "<td>&#8377;90.00</td>")}
        <td>&#8377;1,090.00</td>
      </tr>
      <tr>
        <td>2</td><td>Sample Service B</td>
        {(s.ShowItemHSNCode ? "<td>9983</td>" : "")}
        <td>1</td><td>&#8377;90.00</td>
        {(s.ShowItemDiscount ? "<td>&#8377;0.00</td>" : "")}
        {(s.ShowTaxBreakdown ? "<td>&#8377;8.10</td><td>&#8377;8.10</td>" : "<td>&#8377;16.20</td>")}
        <td>&#8377;106.20</td>
      </tr>
    </tbody>
  </table>

  <div class=""totals"">
    <div class=""totals-box"">
      <div class=""totals-row""><span>Sub Total</span><span>&#8377;1,090.00</span></div>
      {(s.ShowTaxBreakdown ? "<div class=\"totals-row\"><span>CGST</span><span>&#8377;53.10</span></div><div class=\"totals-row\"><span>SGST</span><span>&#8377;53.10</span></div>" : "<div class=\"totals-row\"><span>Tax</span><span>&#8377;106.20</span></div>")}
      {(s.ShowItemDiscount ? "<div class=\"totals-row\"><span>Discount</span><span>- &#8377;0.00</span></div>" : "")}
      {(s.ShowRoundOff ? "<div class=\"totals-row\"><span>Round Off</span><span>+ &#8377;0.70</span></div>" : "")}
      <div class=""totals-row grand""><span>Grand Total</span><span>&#8377;1,197.00</span></div>
      {(s.ShowDueAmount ? "<div class=\"totals-row\" style='color:#e53e3e'><span>Balance Due</span><span>&#8377;1,197.00</span></div>" : "")}
    </div>
  </div>

  {(s.ShowAmountInWords ? $"<div style='margin-top:10px;font-size:{ftrPx + 1}px;color:#555'><em>Rupees One Thousand One Hundred Ninety Seven Only</em></div>" : "")}
  {(s.ShowBankDetails && s.BankDetails != null ? $"<div style='margin-top:10px;font-size:{ftrPx + 1}px;padding:8px;background:#f9fafb;border-radius:4px'><strong>Bank Details:</strong> {s.BankDetails}</div>" : "")}
  {(s.ShowPaymentDetails ? $"<div style='margin-top:8px;font-size:{ftrPx + 1}px;color:#555'>Payment: Cash</div>" : "")}

  {(s.ShowTermsAndConditions ? "<div class=\"footer-note\"><strong>Terms &amp; Conditions:</strong> Goods once sold are not returnable. Payment due within 30 days.</div>" : "")}
  {(s.ShowFooterNote ? $"<div class=\"footer-note\" style='margin-top:6px'>{(s.FooterText ?? "Thank you for your business!")}</div>" : "")}
  {(s.ShowSignatureLine ? $"<div style='margin-top:32px;text-align:right;font-size:{ftrPx + 1}px;color:#555'>Authorised Signature<br><div style='width:160px;border-top:1px solid #999;margin-top:30px;margin-left:auto'></div></div>" : "")}
  {(s.ShowPageNumbers ? "<div class=\"footer-note\" style='text-align:right;margin-top:8px'>Page 1 of 1</div>" : "")}
  {(s.ShowPrintDateTime ? $"<div style='font-size:{ftrPx}px;color:#aaa;text-align:right'>Printed on {DateTime.Now:dd-MMM-yyyy HH:mm}</div>" : "")}
</div>
</body>
</html>";
        }

        // ── Lookup Data ───────────────────────────────────────────────────────────

        public List<PaperSizeInfo> GetPaperSizes() => new()
        {
            new() { Value = "A4",          Label = "A4 (210 × 297 mm)",          WidthMm = 210, HeightMm = 297,  IsThermal = false },
            new() { Value = "A5",          Label = "A5 (148 × 210 mm)",          WidthMm = 148, HeightMm = 210,  IsThermal = false },
            new() { Value = "Letter",      Label = "Letter (216 × 279 mm)",       WidthMm = 216, HeightMm = 279,  IsThermal = false },
            new() { Value = "Legal",       Label = "Legal (216 × 356 mm)",        WidthMm = 216, HeightMm = 356,  IsThermal = false },
            new() { Value = "Thermal80mm", Label = "Thermal 80 mm Receipt",       WidthMm = 80,  HeightMm = null, IsThermal = true, CharsPerLine = 48 },
            new() { Value = "Thermal58mm", Label = "Thermal 58 mm Receipt",       WidthMm = 58,  HeightMm = null, IsThermal = true, CharsPerLine = 32 },
            new() { Value = "Custom",      Label = "Custom (specify dimensions)", WidthMm = null,HeightMm = null, IsCustom  = true }
        };

        public List<PrinterTypeInfo> GetPrinterTypes() => new()
        {
            new() { Value = "Thermal",   Label = "Thermal Receipt Printer", Description = "POS thermal printers (ESC/POS). Fast receipts, auto-cut, cash drawer support.",  SupportsColor = false, SupportsCut = true,  SupportsCashDrawer = true  },
            new() { Value = "Laser",     Label = "Laser Printer",           Description = "High-quality A4/A5 invoices and reports. Best for office environments.",         SupportsColor = true,  SupportsCut = false, SupportsCashDrawer = false },
            new() { Value = "Inkjet",    Label = "Inkjet Printer",          Description = "Standard colour inkjet for invoices, quotations and labels.",                    SupportsColor = true,  SupportsCut = false, SupportsCashDrawer = false },
            new() { Value = "DotMatrix", Label = "Dot Matrix Printer",      Description = "Impact printers for multi-part carbon-copy forms.",                              SupportsColor = false, SupportsCut = false, SupportsCashDrawer = false },
            new() { Value = "PDF",       Label = "Save as PDF",             Description = "Renders to a downloadable PDF file. No physical printer required.",              SupportsColor = true,  SupportsCut = false, SupportsCashDrawer = false },
            new() { Value = "Browser",   Label = "Browser Print",           Description = "Opens the browser print dialog, works with any installed printer.",              SupportsColor = true,  SupportsCut = false, SupportsCashDrawer = false }
        };

        public List<string> GetConnectionTypes() => new() { "USB", "Network", "Bluetooth", "Cloud" };

        // ── Printer Test ──────────────────────────────────────────────────────────

        public async Task<PrinterTestResult> TestPrinterAsync(int tenantId, PrinterTestRequest request)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (request.ConnectionType.Equals("Network", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(request.IpAddress))
                {
                    using var tcp = new System.Net.Sockets.TcpClient();
                    var cts       = new System.Threading.CancellationTokenSource(request.TimeoutMs);
                    await tcp.ConnectAsync(request.IpAddress, request.Port, cts.Token);
                    sw.Stop();
                    return new PrinterTestResult
                    {
                        Success        = true,
                        Message        = $"Connected to {request.IpAddress}:{request.Port} in {sw.ElapsedMilliseconds} ms.",
                        PrinterTarget  = request.PrinterTarget,
                        ConnectionType = request.ConnectionType,
                        LatencyMs      = sw.ElapsedMilliseconds
                    };
                }

                sw.Stop();
                return new PrinterTestResult
                {
                    Success        = true,
                    Message        = $"Printer '{request.PrinterTarget}' configuration is valid ({request.ConnectionType}). Physical connectivity test is only supported for Network printers.",
                    PrinterTarget  = request.PrinterTarget,
                    ConnectionType = request.ConnectionType,
                    LatencyMs      = null
                };
            }
            catch (Exception ex)
            {
                sw.Stop();
                return new PrinterTestResult
                {
                    Success        = false,
                    Message        = $"Connection failed: {ex.Message}",
                    PrinterTarget  = request.PrinterTarget,
                    ConnectionType = request.ConnectionType,
                    LatencyMs      = sw.ElapsedMilliseconds
                };
            }
        }

        // ── Invoice Print Data ────────────────────────────────────────────────────

        public async Task<InvoicePrintDataResponse> GetInvoicePrintDataAsync(int invoiceId, int tenantId)
        {
            var invoice = await _c.Invoices
                .Include(i => i.Items).ThenInclude(i => i.Product)
                .Include(i => i.Customer)
                .Include(i => i.Branch)
                .FirstOrDefaultAsync(i => i.Id == invoiceId && i.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"Invoice {invoiceId} not found.");

            var biz      = await _c.Businesses.FirstOrDefaultAsync(b => b.Id == tenantId);
            var settings = await ResolveSettingsAsync(tenantId, invoice.BranchId, null);

            var lastJob  = await _c.PrintJobs
                .Where(j => j.TenantId == tenantId
                         && j.DocumentId == invoiceId
                         && j.DocumentType == "Invoice"
                         && j.Status == "Printed")
                .OrderByDescending(j => j.CreatedAt)
                .FirstOrDefaultAsync();

            return new InvoicePrintDataResponse
            {
                InvoiceId          = invoice.Id,
                InvoiceNumber      = invoice.InvoiceNumber,
                InvoiceDate        = invoice.InvoiceDate,
                DueDate            = invoice.DueDate,
                CustomerName       = invoice.Customer?.Name,
                CustomerPhone      = invoice.Customer?.Phone,
                CustomerEmail      = invoice.Customer?.Email,
                CustomerAddress    = invoice.Customer?.Address,
                CustomerGSTIN      = invoice.Customer?.GSTNumber,
                BusinessName       = biz?.Name ?? "Business",
                BusinessAddress    = biz?.Address,
                BusinessPhone      = biz?.Phone,
                BusinessEmail      = biz?.Email,
                BusinessGSTIN      = biz?.GSTNumber,
                LogoUrl            = biz?.LogoUrl,
                SubTotal           = invoice.SubTotal,
                TaxAmount          = invoice.TaxAmount,
                DiscountAmount     = invoice.DiscountAmount,
                RoundOff           = invoice.RoundOff,
                TotalAmount        = invoice.TotalAmount,
                PaidAmount         = invoice.PaidAmount,
                DueAmount          = invoice.DueAmount,
                PaymentMethod      = invoice.PaymentMethod?.ToString(),
                Notes              = invoice.Notes,
                TermsAndConditions = invoice.TermsAndConditions,
                PrintSettings      = MapToResponse(settings),
                PrintJobId         = lastJob?.Id,
                IsReprint          = lastJob != null,
                Items              = invoice.Items.Select((item, idx) => new InvoicePrintItemRow
                {
                    SrNo           = idx + 1,
                    ProductName    = item.ProductName,
                    HSNCode        = item.HSNCode,
                    Quantity       = item.Quantity,
                    UnitPrice      = item.UnitPrice,
                    DiscountAmount = item.DiscountAmount,
                    TaxAmount      = item.TaxAmount,
                    CGSTAmount     = item.CGSTAmount,
                    SGSTAmount     = item.SGSTAmount,
                    TaxPercentage  = item.TaxPercentage,
                    TotalPrice     = item.TotalPrice
                }).ToList()
            };
        }

        // ── Print Job Management ──────────────────────────────────────────────────

        private static PrintJobResponse MapJobToResponse(PrintJob j) => new()
        {
            Id                  = j.Id,
            TenantId            = j.TenantId,
            DocumentType        = j.DocumentType,
            DocumentId          = j.DocumentId,
            DocumentReference   = j.DocumentReference,
            Status              = j.Status,
            PrinterTarget       = j.PrinterTarget,
            PaperSize           = j.PaperSize,
            Copies              = j.Copies,
            RetryCount          = j.RetryCount,
            MaxRetries          = j.MaxRetries,
            ErrorMessage        = j.ErrorMessage,
            PrintedAt           = j.PrintedAt,
            RetryAfter          = j.RetryAfter,
            IsReprint           = j.IsReprint,
            OriginalPrintJobId  = j.OriginalPrintJobId,
            InitiatedByUserId   = j.InitiatedByUserId,
            InitiatedByUserName = j.InitiatedByUser?.FullName,
            BranchId            = j.BranchId,
            CounterId           = j.CounterId,
            CreatedAt           = j.CreatedAt,
            UpdatedAt           = j.UpdatedAt
        };

        public async Task<PrintJobResponse> QueuePrintJobAsync(int tenantId, QueuePrintJobRequest request)
        {
            var settings = await ResolveSettingsAsync(tenantId, request.BranchId, request.CounterId);

            var job = new PrintJob
            {
                TenantId          = tenantId,
                DocumentType      = request.DocumentType,
                DocumentId        = request.DocumentId,
                DocumentReference = request.DocumentReference,
                Status            = "Queued",
                PrinterTarget     = request.PrinterTarget ?? settings.DefaultPrinterName,
                PaperSize         = request.PaperSize    ?? settings.PaperSize,
                Copies            = request.Copies,
                MaxRetries        = request.MaxRetries,
                BranchId          = request.BranchId,
                CounterId         = request.CounterId,
                IsReprint         = false
            };

            _c.PrintJobs.Add(job);
            await _c.SaveChangesAsync();
            return MapJobToResponse(job);
        }

        public async Task<PrintJobResponse> UpdatePrintJobStatusAsync(int jobId, int tenantId, string status, string? errorMessage = null)
        {
            var job = await _c.PrintJobs.FirstOrDefaultAsync(j => j.Id == jobId && j.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"PrintJob {jobId} not found.");

            job.Status    = status;
            job.UpdatedAt = DateTime.UtcNow;

            if (status == "Printed") job.PrintedAt    = DateTime.UtcNow;
            if (status == "Failed")  job.ErrorMessage = errorMessage;

            await _c.SaveChangesAsync();
            return MapJobToResponse(job);
        }

        public async Task<PrintJobResponse> RetryPrintJobAsync(int jobId, int tenantId)
        {
            var job = await _c.PrintJobs.FirstOrDefaultAsync(j => j.Id == jobId && j.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"PrintJob {jobId} not found.");

            if (job.RetryCount >= job.MaxRetries)
                throw new InvalidOperationException($"Job {jobId} has reached its retry limit ({job.MaxRetries}).");

            if (job.Status != "Failed" && job.Status != "Cancelled")
                throw new InvalidOperationException($"Only Failed or Cancelled jobs can be retried. Status: {job.Status}");

            job.RetryCount++;
            job.Status       = "Queued";
            job.ErrorMessage = null;
            job.RetryAfter   = null;
            job.UpdatedAt    = DateTime.UtcNow;
            await _c.SaveChangesAsync();
            return MapJobToResponse(job);
        }

        public async Task<PrintJobResponse> ReprintAsync(int originalJobId, int tenantId, int? userId)
        {
            var original = await _c.PrintJobs.FirstOrDefaultAsync(j => j.Id == originalJobId && j.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"Original PrintJob {originalJobId} not found.");

            var reprint = new PrintJob
            {
                TenantId           = tenantId,
                DocumentType       = original.DocumentType,
                DocumentId         = original.DocumentId,
                DocumentReference  = original.DocumentReference,
                Status             = "Queued",
                PrinterTarget      = original.PrinterTarget,
                PaperSize          = original.PaperSize,
                Copies             = original.Copies,
                MaxRetries         = original.MaxRetries,
                IsReprint          = true,
                OriginalPrintJobId = originalJobId,
                InitiatedByUserId  = userId,
                BranchId           = original.BranchId,
                CounterId          = original.CounterId
            };

            _c.PrintJobs.Add(reprint);
            await _c.SaveChangesAsync();
            return MapJobToResponse(reprint);
        }

        public async Task<List<PrintJobResponse>> GetPrintJobsAsync(int tenantId, string? status = null, string? documentType = null, int page = 1, int pageSize = 20)
        {
            var query = _c.PrintJobs
                .Include(j => j.InitiatedByUser)
                .Where(j => j.TenantId == tenantId);

            if (!string.IsNullOrWhiteSpace(status))       query = query.Where(j => j.Status == status);
            if (!string.IsNullOrWhiteSpace(documentType)) query = query.Where(j => j.DocumentType == documentType);

            var jobs = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return jobs.Select(MapJobToResponse).ToList();
        }

        public async Task<PrintJobResponse?> GetPrintJobByIdAsync(int jobId, int tenantId)
        {
            var job = await _c.PrintJobs
                .Include(j => j.InitiatedByUser)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.TenantId == tenantId);
            return job == null ? null : MapJobToResponse(job);
        }

        public async Task<bool> CancelPrintJobAsync(int jobId, int tenantId)
        {
            var job = await _c.PrintJobs.FirstOrDefaultAsync(j => j.Id == jobId && j.TenantId == tenantId)
                ?? throw new KeyNotFoundException($"PrintJob {jobId} not found.");

            if (job.Status == "Printed")
                throw new InvalidOperationException("Cannot cancel a Printed job. Use Reprint instead.");

            job.Status    = "Cancelled";
            job.UpdatedAt = DateTime.UtcNow;
            await _c.SaveChangesAsync();
            return true;
        }
    }
}
