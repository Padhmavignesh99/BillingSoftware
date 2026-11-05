using MasterBilling.API.Models.Enums;

namespace MasterBilling.API.Models.Entities
{
    public class Invoice : TenantEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
        public PaymentMethod? PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public string? TermsAndConditions { get; set; }
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
        public int? SalesPersonId { get; set; }
        public User? SalesPerson { get; set; }
        public string? PdfUrl { get; set; }
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public class InvoiceItem : TenantEntity
    {
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? HSNCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public DiscountType? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal? TaxPercentage { get; set; }
        public decimal? CGSTAmount { get; set; }
        public decimal? SGSTAmount { get; set; }
        public decimal? IGSTAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public int? TaxRateId { get; set; }
        public TaxRate? TaxRate { get; set; }
    }

    public class SalesReturn : TenantEntity
    {
        public string ReturnNumber { get; set; } = string.Empty;
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string Status { get; set; } = "Pending";
        public ICollection<SalesReturnItem> Items { get; set; } = new List<SalesReturnItem>();
    }

    public class SalesReturnItem : TenantEntity
    {
        public int SalesReturnId { get; set; }
        public SalesReturn? SalesReturn { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class Quotation : TenantEntity
    {
        public string QuotationNumber { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public DateTime QuotationDate { get; set; } = DateTime.UtcNow;
        public DateTime? ValidUntil { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public QuotationStatus Status { get; set; } = QuotationStatus.Draft;
        public string? Notes { get; set; }
        public int? ConvertedInvoiceId { get; set; }
        public Invoice? ConvertedInvoice { get; set; }
        public ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
    }

    public class QuotationItem : TenantEntity
    {
        public int QuotationId { get; set; }
        public Quotation? Quotation { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class SalesOrder : TenantEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveryDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
        public string? Notes { get; set; }
        public string? ShippingAddress { get; set; }
        public int? ConvertedInvoiceId { get; set; }
        public Invoice? ConvertedInvoice { get; set; }
        public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
    }

    public class SalesOrderItem : TenantEntity
    {
        public int SalesOrderId { get; set; }
        public SalesOrder? SalesOrder { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
