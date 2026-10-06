using BillingSoftware.API.Models.Enums;

namespace BillingSoftware.API.DTOs.Requests
{
    // ==================== INVENTORY ====================
    public class AdjustStockRequest
    {
        public decimal Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class CreateStockAdjustmentRequest
    {
        public int ProductId { get; set; }
        public decimal QuantityAdjusted { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int? BranchId { get; set; }
    }

    public class CreateStockTransferRequest
    {
        public int ProductId { get; set; }
        public int FromBranchId { get; set; }
        public int ToBranchId { get; set; }
        public int? FromWarehouseId { get; set; }
        public int? ToWarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateWarehouseRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Phone { get; set; }
        public string? ManagerName { get; set; }
        public int? BranchId { get; set; }
    }

    public class UpdateWarehouseRequest
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Phone { get; set; }
        public string? ManagerName { get; set; }
    }

    // ==================== PURCHASE ====================
    public class CreatePurchaseRequest
    {
        public int SupplierId { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Notes { get; set; }
        public string? ReferenceNumber { get; set; }
        public int? BranchId { get; set; }
        public List<CreatePurchaseItemRequest> Items { get; set; } = new();
    }

    public class UpdatePurchaseRequest
    {
        public DateTime? DueDate { get; set; }
        public string? Notes { get; set; }
        public string? ReferenceNumber { get; set; }
        public List<CreatePurchaseItemRequest>? Items { get; set; }
    }

    public class CreatePurchaseItemRequest
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int? TaxRateId { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public DateTime? ExpiryDate { get; set; }
        public string? BatchNumber { get; set; }
    }

    public class CreatePurchaseOrderRequest
    {
        public int SupplierId { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public int? BranchId { get; set; }
        public List<CreatePurchaseOrderItemRequest> Items { get; set; } = new();
    }

    public class UpdatePurchaseOrderRequest
    {
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public PurchaseOrderStatus? Status { get; set; }
        public List<CreatePurchaseOrderItemRequest>? Items { get; set; }
    }

    public class CreatePurchaseOrderItemRequest
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreatePurchasePaymentRequest
    {
        public int PurchaseId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }

    public class CreatePurchaseReturnRequest
    {
        public int PurchaseId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<CreatePurchaseReturnItemRequest> Items { get; set; } = new();
    }

    public class CreatePurchaseReturnItemRequest
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreateGoodsReceiptRequest
    {
        public int? PurchaseOrderId { get; set; }
        public int? PurchaseId { get; set; }
        public int SupplierId { get; set; }
        public string? Notes { get; set; }
        public List<CreateGoodsReceiptItemRequest> Items { get; set; } = new();
    }

    public class CreateGoodsReceiptItemRequest
    {
        public int ProductId { get; set; }
        public decimal OrderedQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal? DamagedQuantity { get; set; }
        public string? Notes { get; set; }
    }

    // ==================== CUSTOMER ====================
    public class CreateCustomerRequest
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
        public decimal CreditLimit { get; set; } = 0;
        public string? Notes { get; set; }
    }

    public class UpdateCustomerRequest
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PinCode { get; set; }
        public string? GSTNumber { get; set; }
        public string? PANNumber { get; set; }
        public int? CustomerGroupId { get; set; }
        public decimal? CreditLimit { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateCustomerGroupRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? DiscountPercentage { get; set; }
    }

    public class CreateCustomerPaymentRequest
    {
        public int CustomerId { get; set; }
        public int? InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }

    // ==================== SUPPLIER ====================
    public class CreateSupplierRequest
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
        public decimal CreditLimit { get; set; } = 0;
        public int CreditDays { get; set; } = 30;
        public string? Notes { get; set; }
    }

    public class UpdateSupplierRequest
    {
        public string? Name { get; set; }
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
        public decimal? CreditLimit { get; set; }
        public int? CreditDays { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateSupplierGroupRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CreateSupplierPaymentRequest
    {
        public int SupplierId { get; set; }
        public int? PurchaseId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }
}
