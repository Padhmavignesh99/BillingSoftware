using BillingSoftware.API.Models.Enums;

namespace BillingSoftware.API.Models.Entities
{
    public class Payment : TenantEntity
    {
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? ReferenceNumber { get; set; }
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
    }

    public class PaymentMethodEntity : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; } = false;
        public string? Icon { get; set; }
        public int SortOrder { get; set; } = 0;
    }

    public class Refund : TenantEntity
    {
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public RefundStatus Status { get; set; } = RefundStatus.Pending;
        public PaymentMethod RefundMethod { get; set; }
        public DateTime RefundDate { get; set; } = DateTime.UtcNow;
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }

    public class CashRegister : TenantEntity
    {
        public string RegisterName { get; set; } = string.Empty;
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
        public int OpenedByUserId { get; set; }
        public User? OpenedByUser { get; set; }
        public int? ClosedByUserId { get; set; }
        public User? ClosedByUser { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalRefunds { get; set; }
        public decimal TotalCashIn { get; set; }
        public decimal TotalCashOut { get; set; }
        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }
        public CashRegisterStatus Status { get; set; } = CashRegisterStatus.Open;
        public string? Notes { get; set; }
    }

    public class PaymentGatewayTransaction : TenantEntity
    {
        public string GatewayName { get; set; } = string.Empty;
        public string? GatewayTransactionId { get; set; }
        public string? OrderId { get; set; }
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; } = "INR";
        public string Status { get; set; } = "Pending";
        public string? PaymentLink { get; set; }
        public string? CallbackData { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
