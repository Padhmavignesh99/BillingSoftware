namespace BillingSoftware.API.DTOs.Requests
{
    // ==================== ADMIN — SHOP ====================
    public class AdminUpdateShopRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? GSTNumber { get; set; }
    }

    // ==================== ADMIN — PLAN ====================
    public class AdminCreatePlanRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal? YearlyPrice { get; set; }
        public int MaxProducts { get; set; } = 100;
        public int MaxUsers { get; set; } = 3;
        public int MaxBranches { get; set; } = 1;
        public int MaxInvoicesPerMonth { get; set; } = 500;
        public bool HasWhatsAppIntegration { get; set; } = false;
        public bool HasEmailIntegration { get; set; } = true;
        public bool HasReports { get; set; } = true;
        public bool HasMultiCurrency { get; set; } = false;
        public bool HasInventoryManagement { get; set; } = true;
        public bool HasDeviceIntegration { get; set; } = false;
        public bool HasApiAccess { get; set; } = false;
        public int TrialDays { get; set; } = 30;
        public bool IsActive { get; set; } = true;
        public bool IsPopular { get; set; } = false;
        public int SortOrder { get; set; } = 0;
    }

    // ==================== ADMIN — PLAN FEATURE ====================
    public class AdminPlanFeatureRequest
    {
        public string FeatureName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? FeatureValue { get; set; }
    }

    // ==================== ADMIN — SUBSCRIPTION ====================
    public class AdminAssignSubscriptionRequest
    {
        public int BusinessId { get; set; }
        public int PlanId { get; set; }
        public string BillingCycle { get; set; } = "Monthly";
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public decimal AmountPaid { get; set; }
        public string? Notes { get; set; }
        public bool AutoRenew { get; set; } = true;
    }

    public class AdminUpdateSubscriptionStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }

    public class AdminExtendSubscriptionRequest
    {
        public int Days { get; set; }
        public string? Reason { get; set; }
    }

    // ==================== ADMIN — PAYMENTS ====================
    public class AdminRecordPaymentRequest
    {
        public int BusinessId { get; set; }
        public int SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; } = "INR";
        public string? PaymentGateway { get; set; }
        public string? TransactionId { get; set; }
        public string Status { get; set; } = "Completed";
        public DateTime? PaymentDate { get; set; }
        public string? Notes { get; set; }
    }

    // ==================== ADMIN — TRIAL ====================
    public class AdminStartTrialRequest
    {
        public int TrialDays { get; set; } = 30;
    }

    public class AdminExtendTrialRequest
    {
        public int Days { get; set; }
        public string? Reason { get; set; }
    }

    // ==================== USER / AUTH — TRIAL REGISTRATION ====================
    /// <summary>
    /// Request to start a free trial after account creation.
    /// </summary>
    public class StartTrialRequest
    {
        public string ContactName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? BusinessAddress { get; set; }
        public string? BusinessCity { get; set; }
        public string? BusinessState { get; set; }
        public string? GSTNumber { get; set; }
    }

    // ==================== USER / AUTH — PURCHASE PLAN ====================
    /// <summary>
    /// Request to subscribe to a paid plan.
    /// </summary>
    public class PurchasePlanRequest
    {
        public int PlanId { get; set; }
        public string BillingCycle { get; set; } = "Monthly";
        public string? PaymentGateway { get; set; }
        public string? TransactionId { get; set; }
        public decimal AmountPaid { get; set; }
    }
}
