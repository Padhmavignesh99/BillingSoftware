namespace BillingSoftware.API.DTOs.Requests
{
    // ── ADMIN: SHOP ──────────────────────────────────────────────────────────────
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

    // ── ADMIN: USER ───────────────────────────────────────────────────────────────
    public class AdminUpdateUserRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
    }

    public class AdminResetPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }

    // ── ADMIN: SUBSCRIPTION PLAN ─────────────────────────────────────────────────
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
        public bool HasWhatsAppIntegration { get; set; }
        public bool HasSmsIntegration { get; set; }
        public bool HasEmailIntegration { get; set; } = true;
        public bool HasReports { get; set; } = true;
        public bool HasMultiCurrency { get; set; }
        public bool HasInventoryManagement { get; set; } = true;
        public bool HasDeviceIntegration { get; set; }
        public bool HasApiAccess { get; set; }
        public int TrialDays { get; set; } = 30;
        public bool IsActive { get; set; } = true;
        public bool IsPopular { get; set; }
        public int SortOrder { get; set; }
        public List<string>? Features { get; set; }
    }

    public class AdminUpdatePlanRequest : AdminCreatePlanRequest { }

    // ── ADMIN: PLAN FEATURES ──────────────────────────────────────────────────────
    public class AdminPlanFeatureRequest
    {
        public string FeatureName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? FeatureValue { get; set; }
    }

    // ── ADMIN: SUBSCRIPTION ASSIGN ───────────────────────────────────────────────
    public class AdminGrantSubscriptionRequest
    {
        public int PlanId { get; set; }
        public int DurationMonths { get; set; } = 1;
        public string? Note { get; set; }
    }

    public class AdminAssignSubscriptionRequest
    {
        public int BusinessId { get; set; }
        public int PlanId { get; set; }
        public string BillingCycle { get; set; } = "Monthly";
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);
        public decimal AmountPaid { get; set; } = 0;
        public bool AutoRenew { get; set; } = true;
        public string? Note { get; set; }
    }

    public class AdminExtendSubscriptionRequest
    {
        public int Days { get; set; } = 30;
    }

    public class AdminUpdateSubscriptionStatusRequest
    {
        /// <summary>1=Trial,2=Active,3=Expired,4=Cancelled,5=Suspended,6=LifetimeFree,7=MonthlyFree</summary>
        public string Status { get; set; } = string.Empty;
    }

    public class AdminExtendTrialRequest
    {
        public int Days { get; set; } = 14;
    }

    public class AdminStartTrialRequest
    {
        public int TrialDays { get; set; } = 30;
    }

    // ── ADMIN: PAYMENT ────────────────────────────────────────────────────────────
    public class AdminRecordPaymentRequest
    {
        public int BusinessId { get; set; }
        public int SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; } = "INR";
        public string? PaymentGateway { get; set; }
        public string? TransactionId { get; set; }
        public string? Status { get; set; } = "Completed";
        public DateTime? PaymentDate { get; set; }
        public string? Notes { get; set; }
    }

    // ── SUBSCRIPTION: PURCHASE (user-facing) ─────────────────────────────────────
    public class PurchasePlanRequest
    {
        public int PlanId { get; set; }
        public string BillingCycle { get; set; } = "Monthly";
        public string? PaymentGateway { get; set; }
        public string? TransactionId { get; set; }
        public decimal AmountPaid { get; set; }
    }

    // ── SUBSCRIPTION: START TRIAL (user-facing) ───────────────────────────────────
    public class StartTrialRequest
    {
        public string? BusinessAddress { get; set; }
        public string? BusinessCity { get; set; }
        public string? BusinessState { get; set; }
        public string? GSTNumber { get; set; }
        public string? Phone { get; set; }
    }

    // ── ADMIN: PLAN PERMIT (SuperAdmin grants free access) ───────────────────────
    /// <summary>SuperAdmin grants a business free plan access.</summary>
    public class GrantPermitRequest
    {
        /// <summary>"OneMonth" or "LifetimeFree"</summary>
        public string PermitDuration { get; set; } = "OneMonth";
        /// <summary>Optional internal note (e.g. "Beta tester", "Partner deal").</summary>
        public string? Note { get; set; }
    }
}
