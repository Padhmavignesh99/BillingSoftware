using BillingSoftware.API.Models.Enums;

namespace BillingSoftware.API.DTOs.Requests
{
    // ==================== INVOICE ====================
    public class CreateInvoiceRequest
    {
        public int? CustomerId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal RoundOff { get; set; } = 0;
        public PaymentMethod? PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public string? TermsAndConditions { get; set; }
        public int? BranchId { get; set; }
        public int? SalesPersonId { get; set; }
        public List<CreateInvoiceItemRequest> Items { get; set; } = new();
    }

    public class UpdateInvoiceRequest
    {
        public DateTime? DueDate { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? RoundOff { get; set; }
        public string? Notes { get; set; }
        public string? TermsAndConditions { get; set; }
        public InvoiceStatus? Status { get; set; }
        public List<CreateInvoiceItemRequest>? Items { get; set; }
    }

    public class CreateInvoiceItemRequest
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DiscountType? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public int? TaxRateId { get; set; }
    }

    // ==================== SALES RETURN ====================
    public class CreateSalesReturnRequest
    {
        public int InvoiceId { get; set; }
        public int? CustomerId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<CreateSalesReturnItemRequest> Items { get; set; } = new();
    }

    public class CreateSalesReturnItemRequest
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    // ==================== QUOTATION ====================
    public class CreateQuotationRequest
    {
        public int? CustomerId { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string? Notes { get; set; }
        public List<CreateQuotationItemRequest> Items { get; set; } = new();
    }

    public class UpdateQuotationRequest
    {
        public DateTime? ValidUntil { get; set; }
        public string? Notes { get; set; }
        public QuotationStatus? Status { get; set; }
        public List<CreateQuotationItemRequest>? Items { get; set; }
    }

    public class CreateQuotationItemRequest
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public int? TaxRateId { get; set; }
    }

    // ==================== SALES ORDER ====================
    public class CreateSalesOrderRequest
    {
        public int? CustomerId { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? Notes { get; set; }
        public string? ShippingAddress { get; set; }
        public List<CreateSalesOrderItemRequest> Items { get; set; } = new();
    }

    public class UpdateSalesOrderRequest
    {
        public DateTime? DeliveryDate { get; set; }
        public string? Notes { get; set; }
        public string? ShippingAddress { get; set; }
        public SalesOrderStatus? Status { get; set; }
        public List<CreateSalesOrderItemRequest>? Items { get; set; }
    }

    public class CreateSalesOrderItemRequest
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public int? TaxRateId { get; set; }
    }

    // ==================== POS ====================
    public class POSBillRequest
    {
        public int? CustomerId { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public decimal? AmountReceived { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public int? BranchId { get; set; }
        public List<POSItemRequest> Items { get; set; } = new();
    }

    public class POSItemRequest
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
    }

    // ==================== PAYMENT ====================
    public class CreatePaymentRequest
    {
        public int? InvoiceId { get; set; }
        public int? CustomerId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public int? BranchId { get; set; }
    }

    public class CollectDueRequest
    {
        public int CustomerId { get; set; }
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }

    // ==================== REFUND ====================
    public class CreateRefundRequest
    {
        public int? InvoiceId { get; set; }
        public int? PaymentId { get; set; }
        public int? CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public PaymentMethod RefundMethod { get; set; }
        public string? Notes { get; set; }
    }

    // ==================== CASH REGISTER ====================
    public class OpenCashRegisterRequest
    {
        public string RegisterName { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public int? BranchId { get; set; }
    }

    public class CloseCashRegisterRequest
    {
        public int RegisterId { get; set; }
        public decimal ClosingBalance { get; set; }
        public string? Notes { get; set; }
    }

    // ==================== PAYMENT METHOD ====================
    public class CreatePaymentMethodRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; } = false;
        public string? Icon { get; set; }
        public int SortOrder { get; set; } = 0;
    }

    // ==================== EXPENSE ====================
    public class CreateExpenseRequest
    {
        public int? ExpenseCategoryId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? ExpenseDate { get; set; }
        public string? Description { get; set; }
        public string? ReferenceNumber { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public string? PaidTo { get; set; }
        public int? BranchId { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateExpenseRequest
    {
        public int? ExpenseCategoryId { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? ExpenseDate { get; set; }
        public string? Description { get; set; }
        public string? ReferenceNumber { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public string? PaidTo { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateExpenseCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int? ParentCategoryId { get; set; }
    }

    public class CreateExpensePaymentRequest
    {
        public int ExpenseId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }

    // ==================== CREDIT BILL ====================
    public class CreateCreditBillRequest
    {
        public int CustomerId { get; set; }
        public int? InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Notes { get; set; }
        /// <summary>The billing user/staff ID who is creating this credit sale.</summary>
        public int? UserId { get; set; }
    }

    public class SettleCreditRequest
    {
        public int CreditId { get; set; }
        public decimal PaymentAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public string? Notes { get; set; }
    }
}
