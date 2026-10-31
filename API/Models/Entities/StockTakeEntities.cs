namespace BillingSoftware.API.Models.Entities
{
    public class StockTakeSession : TenantEntity
    {
        public string SessionNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
        public int? WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        public string Status { get; set; } = "InProgress"; // InProgress, Completed, Cancelled
        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }
        public int TotalProducts { get; set; }
        public int CountedProducts { get; set; }
        public int DiscrepancyCount { get; set; }
        public ICollection<StockTakeItem> Items { get; set; } = new List<StockTakeItem>();
    }

    public class StockTakeItem : TenantEntity
    {
        public int StockTakeSessionId { get; set; }
        public StockTakeSession? Session { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public decimal SystemStock { get; set; }
        public decimal? CountedStock { get; set; }
        public decimal? Discrepancy { get; set; }
        public bool IsAdjusted { get; set; } = false;
        public string? Notes { get; set; }
    }
}

