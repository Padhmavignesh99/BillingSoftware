namespace MasterBilling.API.Models.Entities
{
    public class Supplier : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PinCode { get; set; }
        public string? GSTNumber { get; set; }
        public string? PANNumber { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIFSC { get; set; }
        public int? SupplierGroupId { get; set; }
        public SupplierGroup? SupplierGroup { get; set; }
        public decimal CreditLimit { get; set; } = 0;
        public decimal OutstandingBalance { get; set; } = 0;
        public int CreditDays { get; set; } = 30;
        public string? Notes { get; set; }
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }

    public class SupplierGroup : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
    }

    public class SupplierLedger : TenantEntity
    {
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string TransactionType { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public int? ReferenceId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal RunningBalance { get; set; }
        public string? Notes { get; set; }
    }

    public class SupplierPayment : TenantEntity
    {
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public int? PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }
        public decimal Amount { get; set; }
        public Enums.PaymentMethod PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Completed";
    }

    public class SupplierCredit : TenantEntity
    {
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public int? PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; } = 0;
        public decimal BalanceAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }
    }
}
