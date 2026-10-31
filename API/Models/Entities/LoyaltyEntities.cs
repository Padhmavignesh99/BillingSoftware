namespace BillingSoftware.API.Models.Entities
{
    public class LoyaltyProgram : TenantEntity
    {
        public string Name { get; set; } = "Loyalty Program";
        public bool IsEnabled { get; set; } = false;
        public decimal PointsPerRupee { get; set; } = 1; // Points earned per rupee spent
        public decimal RupeePerPoint { get; set; } = 0.1m; // Rupee value of each point
        public decimal MinRedemptionPoints { get; set; } = 100;
        public decimal MaxRedemptionPercent { get; set; } = 20; // Max % of invoice value redeemable
        public int PointsExpiryDays { get; set; } = 365;
        public string? TermsAndConditions { get; set; }
    }

    public class LoyaltyTier : TenantEntity
    {
        public int LoyaltyProgramId { get; set; }
        public LoyaltyProgram? LoyaltyProgram { get; set; }
        public string Name { get; set; } = string.Empty; // Bronze, Silver, Gold, Platinum
        public decimal MinPoints { get; set; }
        public decimal MaxPoints { get; set; }
        public decimal BonusMultiplier { get; set; } = 1;
        public string? BadgeColor { get; set; }
        public string? Benefits { get; set; }
        public int SortOrder { get; set; } = 0;
    }

    public class CustomerLoyaltyAccount : TenantEntity
    {
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public int LoyaltyProgramId { get; set; }
        public LoyaltyProgram? LoyaltyProgram { get; set; }
        public int? LoyaltyTierId { get; set; }
        public LoyaltyTier? LoyaltyTier { get; set; }
        public decimal TotalEarnedPoints { get; set; }
        public decimal TotalRedeemedPoints { get; set; }
        public decimal CurrentPoints { get; set; }
        public decimal LifetimeSpend { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public ICollection<LoyaltyTransaction> Transactions { get; set; } = new List<LoyaltyTransaction>();
    }

    public class LoyaltyTransaction : TenantEntity
    {
        public int CustomerLoyaltyAccountId { get; set; }
        public CustomerLoyaltyAccount? LoyaltyAccount { get; set; }
        public string Type { get; set; } = "Earn"; // Earn, Redeem, Adjust, Expire
        public decimal Points { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public string? Notes { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }
}

