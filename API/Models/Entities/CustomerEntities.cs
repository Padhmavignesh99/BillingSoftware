namespace MasterBilling.API.Models.Entities
{
    public class Customer : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PinCode { get; set; }
        public string? GSTNumber { get; set; }
        public string? PANNumber { get; set; }
        public int? CustomerGroupId { get; set; }
        public CustomerGroup? CustomerGroup { get; set; }
        public decimal CreditLimit { get; set; } = 0;
        public decimal OutstandingBalance { get; set; } = 0;
        public decimal TotalPurchaseAmount { get; set; } = 0;
        public int TotalPurchaseCount { get; set; } = 0;
        public string? Notes { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }

    public class CustomerGroup : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }

    public class CustomerLedger : TenantEntity
    {
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string TransactionType { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public int? ReferenceId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal RunningBalance { get; set; }
        public string? Notes { get; set; }
    }

    public class CustomerPayment : TenantEntity
    {
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public decimal Amount { get; set; }
        public Enums.PaymentMethod PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Completed";
    }

    public class CustomerCredit : TenantEntity
    {
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; } = 0;
        public decimal BalanceAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }
        /// <summary>User (billing staff) who created this credit bill — for user-wise credit balance tracking.</summary>
        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}
