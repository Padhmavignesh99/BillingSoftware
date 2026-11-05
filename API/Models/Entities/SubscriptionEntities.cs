using MasterBilling.API.Models.Enums;

namespace MasterBilling.API.Models.Entities
{
    public class SubscriptionPlan : BaseEntity
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
        public bool HasSmsIntegration { get; set; } = false;
        public bool HasEmailIntegration { get; set; } = true;
        public bool HasReports { get; set; } = true;
        public bool HasMultiCurrency { get; set; } = false;
        public bool HasInventoryManagement { get; set; } = true;
        public bool HasDeviceIntegration { get; set; } = false;
        public bool HasApiAccess { get; set; } = false;
        public int TrialDays { get; set; } = 30;
        public new bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public bool IsPopular { get; set; } = false;
        public ICollection<PlanFeature> Features { get; set; } = new List<PlanFeature>();
    }

    public class Subscription : BaseEntity
    {
        public int BusinessId { get; set; }
        public Business? Business { get; set; }
        public int PlanId { get; set; }
        public SubscriptionPlan? Plan { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;
        public string? BillingCycle { get; set; } = "Monthly";
        public decimal AmountPaid { get; set; }
        public string? PaymentGatewayId { get; set; }
        public string? SubscriptionGatewayId { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public bool AutoRenew { get; set; } = true;

        // ── SuperAdmin Plan Permit Fields ─────────────────────────────────────────
        /// <summary>Whether this subscription was granted free by a SuperAdmin.</summary>
        public PlanPermitType PlanPermit { get; set; } = PlanPermitType.Paid;
        /// <summary>Free permit duration: 1 Month or Lifetime. Null if PlanPermit = Paid.</summary>
        public PlanPermitDuration? PermitDuration { get; set; }
        /// <summary>True when SuperAdmin explicitly granted free access.</summary>
        public bool IsGrantedBySuperAdmin { get; set; } = false;
        /// <summary>When the SuperAdmin granted this permit.</summary>
        public DateTime? GrantedAt { get; set; }
        /// <summary>Internal note/reason for the grant (e.g. "Beta tester", "Partner").</summary>
        public string? GrantNote { get; set; }
    }

    public class SubscriptionPayment : BaseEntity
    {
        public int SubscriptionId { get; set; }
        public Subscription? Subscription { get; set; }
        public int BusinessId { get; set; }
        public Business? Business { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; } = "INR";
        public string? PaymentGateway { get; set; }
        public string? TransactionId { get; set; }
        public string Status { get; set; } = "Completed";
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? InvoiceUrl { get; set; }
    }

    public class PlanFeature : BaseEntity
    {
        public int PlanId { get; set; }
        public SubscriptionPlan? Plan { get; set; }
        public string FeatureName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? FeatureValue { get; set; }
    }

    public class Trial : BaseEntity
    {
        public int BusinessId { get; set; }
        public Business? Business { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public bool IsConverted { get; set; } = false;
        public int? ConvertedSubscriptionId { get; set; }
        public Subscription? ConvertedSubscription { get; set; }
    }
}
