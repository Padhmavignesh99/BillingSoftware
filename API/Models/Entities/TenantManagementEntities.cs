namespace MasterBilling.API.Models.Entities
{
    // ==================== COUNTER ====================
    /// <summary>Billing counter / POS terminal at a branch.</summary>
    public class Counter : TenantEntity
    {
        public string  Name            { get; set; } = string.Empty;
        public string? Code            { get; set; }
        public int     BranchId        { get; set; }
        public Branch? Branch          { get; set; }
        public string? Description     { get; set; }
        public bool    IsDefault       { get; set; } = false;
        public int?    AssignedUserId  { get; set; }
        public User?   AssignedUser    { get; set; }
        public string? DeviceId        { get; set; }
        public string? IpAddress       { get; set; }
        public bool    IsOnline        { get; set; } = false;
        public DateTime? LastSeenAt   { get; set; }
    }

    // ==================== EMPLOYEE ROLE ====================
    /// <summary>Named role within a tenant (e.g. Cashier, Manager, Inventory Clerk).</summary>
    public class EmployeeRole : TenantEntity
    {
        public string  Name             { get; set; } = string.Empty;
        public string? Description      { get; set; }
        public bool    IsSystemRole     { get; set; } = false;
        public string? Color            { get; set; }
        public ICollection<RolePermission> Permissions  { get; set; } = new List<RolePermission>();
        public ICollection<TenantUserRole>  UserRoles    { get; set; } = new List<TenantUserRole>();
    }

    // ==================== TENANT USER ROLE (tenant-scoped assignment) ====================
    /// <summary>Assigns a user to an EmployeeRole within a specific tenant (and optionally branch/counter).</summary>
    public class TenantUserRole : TenantEntity
    {
        public int          UserId     { get; set; }
        public User?        User       { get; set; }
        public int          RoleId     { get; set; }
        public EmployeeRole? Role      { get; set; }
        public int?         BranchId   { get; set; }
        public Branch?      Branch     { get; set; }
        public int?         CounterId  { get; set; }
        public Counter?     Counter    { get; set; }
        public DateTime?    ValidFrom  { get; set; }
        public DateTime?    ValidUntil { get; set; }
    }

    // ==================== SUBSCRIPTION USAGE ====================
    /// <summary>Tracks real-time usage metrics against plan limits per month.</summary>
    public class SubscriptionUsage : TenantEntity
    {
        public int    SubscriptionId      { get; set; }
        public Subscription? Subscription { get; set; }
        public int    Month               { get; set; }
        public int    Year                { get; set; }
        public int    InvoicesCreated     { get; set; } = 0;
        public int    ProductsCount       { get; set; } = 0;
        public int    UsersCount          { get; set; } = 0;
        public int    BranchesCount       { get; set; } = 0;
        public int    ApiCallsCount       { get; set; } = 0;
        public long   StorageUsedBytes    { get; set; } = 0;
        public DateTime LastUpdatedAt     { get; set; } = DateTime.UtcNow;
    }

    // ==================== AI INSIGHT ====================
    /// <summary>Generated AI insight (sales summary, anomaly alert, customer insight, etc.).</summary>
    public class AIInsight : TenantEntity
    {
        public string  InsightType         { get; set; } = string.Empty;
        public string  Title               { get; set; } = string.Empty;
        public string  Content             { get; set; } = string.Empty;
        public string? DataJson            { get; set; }
        public string? RecommendationJson  { get; set; }
        public DateTime PeriodStart        { get; set; }
        public DateTime PeriodEnd          { get; set; }
        public decimal? ConfidenceScore    { get; set; }
        public bool    IsRead              { get; set; } = false;
        public bool    IsActedOn           { get; set; } = false;
        public int?    RelatedEntityId     { get; set; }
        public string? RelatedEntityType   { get; set; }
        public int?    GeneratedByUserId   { get; set; }
        public User?   GeneratedByUser     { get; set; }
    }

    // ==================== AI FORECAST ====================
    /// <summary>Product/category demand forecast from the AI engine.</summary>
    public class AIForecast : TenantEntity
    {
        public int       ProductId      { get; set; }
        public Product?  Product        { get; set; }
        public int?      CategoryId     { get; set; }
        public DateTime  ForecastDate   { get; set; }
        public string    Granularity    { get; set; } = "Day";
        public decimal   ForecastedQty  { get; set; }
        public decimal   ActualQty      { get; set; } = 0;
        public decimal?  ConfidenceLow  { get; set; }
        public decimal?  ConfidenceHigh { get; set; }
        public string?   ModelVersion   { get; set; }
        public string?   MetricsJson    { get; set; }
    }

    // ==================== SHOP INVITE ====================
    /// <summary>Invitation token for adding a user to a tenant (shop).</summary>
    public class ShopInvite : TenantEntity
    {
        public string   Email              { get; set; } = string.Empty;
        public string   Token              { get; set; } = Guid.NewGuid().ToString("N");
        public int?     RoleId             { get; set; }
        public EmployeeRole? Role          { get; set; }
        public int?     BranchId           { get; set; }
        public Branch?  Branch             { get; set; }
        public DateTime ExpiresAt          { get; set; } = DateTime.UtcNow.AddDays(7);
        public bool     IsAccepted         { get; set; } = false;
        public DateTime? AcceptedAt        { get; set; }
        public int?     AcceptedByUserId   { get; set; }
        public User?    AcceptedByUser     { get; set; }
        public int      InvitedByUserId    { get; set; }
        public User?    InvitedByUser      { get; set; }
    }

    // ==================== USAGE ALERT ====================
    /// <summary>Fired when a tenant exceeds a usage threshold (80%, 90%, 100% of plan limit).</summary>
    public class UsageAlert : TenantEntity
    {
        public string   AlertType       { get; set; } = string.Empty;
        public int      ThresholdPct    { get; set; }
        public decimal  CurrentValue    { get; set; }
        public decimal  LimitValue      { get; set; }
        public bool     IsAcknowledged  { get; set; } = false;
        public DateTime? AcknowledgedAt { get; set; }
        public int?     AcknowledgedBy  { get; set; }
    }
}
