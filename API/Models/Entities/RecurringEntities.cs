namespace MasterBilling.API.Models.Entities
{
    public class RecurringInvoice : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public string Frequency { get; set; } = "Monthly"; // Daily, Weekly, Monthly, Quarterly, Yearly
        public int FrequencyValue { get; set; } = 1;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public DateTime? NextRunDate { get; set; }
        public DateTime? LastRunDate { get; set; }
        public int RunCount { get; set; } = 0;
        public int? MaxRuns { get; set; }
        public string Status { get; set; } = "Active"; // Active, Paused, Completed, Cancelled
        public string? Notes { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int? BranchId { get; set; }
        public ICollection<RecurringInvoiceItem> Items { get; set; } = new List<RecurringInvoiceItem>();
    }

    public class RecurringInvoiceItem : TenantEntity
    {
        public int RecurringInvoiceId { get; set; }
        public RecurringInvoice? RecurringInvoice { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal? TaxPercentage { get; set; }
        public decimal TotalPrice { get; set; }
    }
}

