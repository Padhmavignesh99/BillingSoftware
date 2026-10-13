using BillingSoftware.API.Models.Enums;

namespace BillingSoftware.API.Models.Entities
{
    public class Inventory : TenantEntity
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int? WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
        public decimal Quantity { get; set; } = 0;
        public decimal? ReorderLevel { get; set; }
        public decimal? ReorderQuantity { get; set; }
    }

    public class StockTransaction : TenantEntity
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public StockTransactionType TransactionType { get; set; }
        public decimal Quantity { get; set; }
        public decimal PreviousStock { get; set; }
        public decimal NewStock { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
        public string? Notes { get; set; }
        public int? WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }

    public class StockAdjustment : TenantEntity
    {
        public string AdjustmentNumber { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal QuantityAdjusted { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow;
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
    }

    public class StockTransfer : TenantEntity
    {
        public string TransferNumber { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int FromBranchId { get; set; }
        public Branch? FromBranch { get; set; }
        public int ToBranchId { get; set; }
        public Branch? ToBranch { get; set; }
        public int? FromWarehouseId { get; set; }
        public Warehouse? FromWarehouse { get; set; }
        public int? ToWarehouseId { get; set; }
        public Warehouse? ToWarehouse { get; set; }
        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
        public DateTime TransferDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
    }

    public class Warehouse : TenantEntity
    {
        public string  Name             { get; set; } = string.Empty;
        public string? Code             { get; set; }
        public string? Description      { get; set; }
        public string? Address          { get; set; }
        public string? City             { get; set; }
        public string? State            { get; set; }
        public string? Phone            { get; set; }
        public string? ManagerName      { get; set; }
        public int?    BranchId         { get; set; }
        public Branch? Branch           { get; set; }
        // ── Extended multi-tenant management fields ─────────────────────────────
        public bool    IsDefault        { get; set; } = false;
        public decimal? StorageCapacity { get; set; }
        public string?  CapacityUnit    { get; set; }  // "m3" | "kg"
        public string?  ContactPerson   { get; set; }
        public string?  ContactPhone    { get; set; }
        public ICollection<StockTransfer> InboundTransfers  { get; set; } = new List<StockTransfer>();
        public ICollection<StockTransfer> OutboundTransfers { get; set; } = new List<StockTransfer>();
    }
}
