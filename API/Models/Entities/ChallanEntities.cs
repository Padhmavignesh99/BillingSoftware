namespace MasterBilling.API.Models.Entities
{
    public class DeliveryChallan : TenantEntity
    {
        public string ChallanNumber { get; set; } = string.Empty;
        public DateTime ChallanDate { get; set; } = DateTime.UtcNow;
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Dispatched, Delivered, Cancelled
        public string? DeliveryAddress { get; set; }
        public string? TransportMode { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? LRNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime? DispatchedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public ICollection<DeliveryChallanItem> Items { get; set; } = new List<DeliveryChallanItem>();
    }

    public class DeliveryChallanItem : TenantEntity
    {
        public int DeliveryChallanId { get; set; }
        public DeliveryChallan? DeliveryChallan { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Notes { get; set; }
    }
}

