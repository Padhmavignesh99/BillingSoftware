using BillingSoftware.API.Models.Enums;

namespace BillingSoftware.API.Models.Entities
{
    public class Purchase : TenantEntity
    {
        public string PurchaseNumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public PurchaseStatus Status { get; set; } = PurchaseStatus.Draft;
        public string? Notes { get; set; }
        public string? ReferenceNumber { get; set; }
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
        public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
        public ICollection<PurchasePayment> Payments { get; set; } = new List<PurchasePayment>();
    }

    public class PurchaseOrder : TenantEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedDeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
        public string? Notes { get; set; }
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }

    public class PurchaseOrderItem : TenantEntity
    {
        public int PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ReceivedQuantity { get; set; } = 0;
    }

    public class PurchaseItem : TenantEntity
    {
        public int PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public int? TaxRateId { get; set; }
        public TaxRate? TaxRate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? BatchNumber { get; set; }
    }

    public class PurchasePayment : TenantEntity
    {
        public int PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }

    public class PurchaseReturn : TenantEntity
    {
        public string ReturnNumber { get; set; } = string.Empty;
        public int PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string Status { get; set; } = "Pending";
        public ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();
    }

    public class PurchaseReturnItem : TenantEntity
    {
        public int PurchaseReturnId { get; set; }
        public PurchaseReturn? PurchaseReturn { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class GoodsReceipt : TenantEntity
    {
        public string ReceiptNumber { get; set; } = string.Empty;
        public int? PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
        public int? PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        public string Status { get; set; } = "Received";
        public ICollection<GoodsReceiptItem> Items { get; set; } = new List<GoodsReceiptItem>();
    }

    public class GoodsReceiptItem : TenantEntity
    {
        public int GoodsReceiptId { get; set; }
        public GoodsReceipt? GoodsReceipt { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal OrderedQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal? DamagedQuantity { get; set; }
        public string? Notes { get; set; }
    }
}
