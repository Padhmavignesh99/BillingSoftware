namespace BillingSoftware.API.DTOs.Requests
{
    // ─── Delivery Challan ─────────────────────────────────────────────────────────
    public class CreateDeliveryChallanRequest
    {
        public int? CustomerId { get; set; }
        public int? InvoiceId { get; set; }
        public int? BranchId { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? TransportMode { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? LRNumber { get; set; }
        public string? Notes { get; set; }
        public List<ChallanItemRequest> Items { get; set; } = new();
    }

    public class ChallanItemRequest
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateChallanStatusRequest
    {
        public string Status { get; set; } = "Dispatched";
    }

    public class DeliveryChallanResponse
    {
        public int Id { get; set; }
        public string ChallanNumber { get; set; } = string.Empty;
        public DateTime ChallanDate { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? DeliveryAddress { get; set; }
        public string? TransportMode { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? LRNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime? DispatchedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public List<ChallanItemResponse> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class ChallanItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Notes { get; set; }
    }

    // ─── Recurring Invoice ────────────────────────────────────────────────────────
    public class CreateRecurringInvoiceRequest
    {
        public string Name { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string Frequency { get; set; } = "Monthly";
        public int FrequencyValue { get; set; } = 1;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public int? MaxRuns { get; set; }
        public string? Notes { get; set; }
        public int? BranchId { get; set; }
        public List<RecurringItemRequest> Items { get; set; } = new();
    }

    public class RecurringItemRequest
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal? TaxPercentage { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class RecurringInvoiceResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public int FrequencyValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextRunDate { get; set; }
        public DateTime? LastRunDate { get; set; }
        public int RunCount { get; set; }
        public int? MaxRuns { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<RecurringItemResponse> Items { get; set; } = new();
    }

    public class RecurringItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    // ─── Stock Take ──────────────────────────────────────────────────────────────
    public class CreateStockTakeRequest
    {
        public string Name { get; set; } = string.Empty;
        public int? BranchId { get; set; }
        public int? WarehouseId { get; set; }
        public string? Notes { get; set; }
        public List<int>? ProductIds { get; set; } // null = all products
    }

    public class UpdateStockTakeItemRequest
    {
        public int ItemId { get; set; }
        public decimal CountedStock { get; set; }
        public string? Notes { get; set; }
    }

    public class StockTakeSessionResponse
    {
        public int Id { get; set; }
        public string SessionNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }
        public int TotalProducts { get; set; }
        public int CountedProducts { get; set; }
        public int DiscrepancyCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<StockTakeItemResponse> Items { get; set; } = new();
    }

    public class StockTakeItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public decimal SystemStock { get; set; }
        public decimal? CountedStock { get; set; }
        public decimal? Discrepancy { get; set; }
        public bool IsAdjusted { get; set; }
        public string? Notes { get; set; }
    }

    // ─── Loyalty ──────────────────────────────────────────────────────────────────
    public class UpdateLoyaltyProgramRequest
    {
        public string Name { get; set; } = "Loyalty Program";
        public bool IsEnabled { get; set; }
        public decimal PointsPerRupee { get; set; } = 1;
        public decimal RupeePerPoint { get; set; } = 0.1m;
        public decimal MinRedemptionPoints { get; set; } = 100;
        public decimal MaxRedemptionPercent { get; set; } = 20;
        public int PointsExpiryDays { get; set; } = 365;
        public string? TermsAndConditions { get; set; }
    }

    public class CreateLoyaltyTierRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal MinPoints { get; set; }
        public decimal MaxPoints { get; set; }
        public decimal BonusMultiplier { get; set; } = 1;
        public string? BadgeColor { get; set; }
        public string? Benefits { get; set; }
        public int SortOrder { get; set; }
    }

    public class EarnPointsRequest
    {
        public int CustomerId { get; set; }
        public int? InvoiceId { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class RedeemPointsRequest
    {
        public int CustomerId { get; set; }
        public decimal Points { get; set; }
        public int? InvoiceId { get; set; }
        public string? Notes { get; set; }
    }

    public class AdjustPointsRequest
    {
        public int CustomerId { get; set; }
        public decimal Points { get; set; } // positive = add, negative = deduct
        public string? Notes { get; set; }
    }

    public class LoyaltyProgramResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public decimal PointsPerRupee { get; set; }
        public decimal RupeePerPoint { get; set; }
        public decimal MinRedemptionPoints { get; set; }
        public decimal MaxRedemptionPercent { get; set; }
        public int PointsExpiryDays { get; set; }
        public string? TermsAndConditions { get; set; }
    }

    public class LoyaltyTierResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal MinPoints { get; set; }
        public decimal MaxPoints { get; set; }
        public decimal BonusMultiplier { get; set; }
        public string? BadgeColor { get; set; }
        public string? Benefits { get; set; }
        public int SortOrder { get; set; }
    }

    public class LoyaltyAccountResponse
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? TierName { get; set; }
        public string? TierBadgeColor { get; set; }
        public decimal TotalEarnedPoints { get; set; }
        public decimal TotalRedeemedPoints { get; set; }
        public decimal CurrentPoints { get; set; }
        public decimal LifetimeSpend { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public List<LoyaltyTransactionResponse> RecentTransactions { get; set; } = new();
    }

    public class LoyaltyTransactionResponse
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Points { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public int? InvoiceId { get; set; }
        public string? Notes { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    public class RedeemCheckResponse
    {
        public bool CanRedeem { get; set; }
        public decimal CurrentPoints { get; set; }
        public decimal RequestedPoints { get; set; }
        public decimal RedemptionValue { get; set; }
        public string? Message { get; set; }
    }

    // ─── Reminders ────────────────────────────────────────────────────────────────
    public class CreateReminderRuleRequest
    {
        public string Name { get; set; } = string.Empty;
        public int DaysOverdue { get; set; }
        public bool SendWhatsApp { get; set; } = true;
        public bool SendEmail { get; set; }
        public bool SendSms { get; set; }
        public string MessageTemplate { get; set; } = "Dear {CustomerName}, your invoice {InvoiceNumber} of ₹{Amount} is overdue.";
        public bool IsEnabled { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public class SendManualReminderRequest
    {
        public int CustomerId { get; set; }
        public int? InvoiceId { get; set; }
        public bool SendWhatsApp { get; set; } = true;
        public bool SendEmail { get; set; }
        public bool SendSms { get; set; }
        public string? CustomMessage { get; set; }
    }

    public class ReminderRuleResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DaysOverdue { get; set; }
        public bool SendWhatsApp { get; set; }
        public bool SendEmail { get; set; }
        public bool SendSms { get; set; }
        public string MessageTemplate { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public int SortOrder { get; set; }
    }

    public class ReminderLogResponse
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? InvoiceId { get; set; }
        public string? Channel { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; }
    }

    public class OverdueCustomerResponse
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public decimal TotalOverdue { get; set; }
        public int OverdueInvoiceCount { get; set; }
        public int MaxDaysOverdue { get; set; }
        public List<OverdueInvoiceItem> Invoices { get; set; } = new();
    }

    public class OverdueInvoiceItem
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal DueAmount { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
    }

    // ─── Reorder ──────────────────────────────────────────────────────────────────
    public class ReorderSuggestion
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal AvgDailySales { get; set; }
        public decimal DaysOfStockLeft { get; set; }
        public decimal SuggestedOrderQty { get; set; }
        public int? PreferredSupplierId { get; set; }
        public string? SupplierName { get; set; }
        public decimal? LastPurchasePrice { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Urgency { get; set; } = "Normal"; // Critical, Urgent, Watch, Normal
    }
}

