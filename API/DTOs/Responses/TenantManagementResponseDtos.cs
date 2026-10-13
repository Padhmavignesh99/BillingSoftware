namespace BillingSoftware.API.DTOs.Responses
{
    // =====================================================================
    // WAREHOUSE
    // =====================================================================
    public class WarehouseResponse
    {
        public int     Id             { get; set; }
        public string  Name           { get; set; } = string.Empty;
        public string? Code           { get; set; }
        public string? Description    { get; set; }
        public int?    BranchId       { get; set; }
        public string? BranchName     { get; set; }
        public string? Address        { get; set; }
        public bool    IsDefault      { get; set; }
        public decimal? StorageCapacity { get; set; }
        public string?  CapacityUnit  { get; set; }
        public string?  ContactPerson { get; set; }
        public string?  ContactPhone  { get; set; }
        public bool    IsActive       { get; set; }
        public DateTime CreatedAt     { get; set; }
    }

    // =====================================================================
    // COUNTER
    // =====================================================================
    public class CounterResponse
    {
        public int     Id             { get; set; }
        public string  Name           { get; set; } = string.Empty;
        public string? Code           { get; set; }
        public int     BranchId       { get; set; }
        public string? BranchName     { get; set; }
        public string? Description    { get; set; }
        public bool    IsDefault      { get; set; }
        public int?    AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }
        public string? DeviceId       { get; set; }
        public string? IpAddress      { get; set; }
        public bool    IsOnline       { get; set; }
        public DateTime? LastSeenAt  { get; set; }
        public bool    IsActive       { get; set; }
        public DateTime CreatedAt     { get; set; }
    }

    // =====================================================================
    // EMPLOYEE ROLE
    // =====================================================================
    public class EmployeeRoleResponse
    {
        public int     Id            { get; set; }
        public string  Name          { get; set; } = string.Empty;
        public string? Description   { get; set; }
        public string? Color         { get; set; }
        public bool    IsSystemRole  { get; set; }
        public int     UserCount     { get; set; }
        public List<PermissionResponse> Permissions { get; set; } = new();
        public bool    IsActive      { get; set; }
        public DateTime CreatedAt    { get; set; }
    }

    // PermissionResponse and PermissionGroupResponse live in ResponseDtos.cs

    public class PermissionGroupResponse
    {
        public string Module { get; set; } = string.Empty;
        public List<PermissionResponse> Permissions { get; set; } = new();
    }

    // =====================================================================
    // USER ROLE
    // =====================================================================
    public class UserRoleResponse
    {
        public int     Id          { get; set; }
        public int     UserId      { get; set; }
        public string  UserName    { get; set; } = string.Empty;
        public string? UserEmail   { get; set; }
        public int     RoleId      { get; set; }
        public string  RoleName    { get; set; } = string.Empty;
        public string? RoleColor   { get; set; }
        public int?    BranchId    { get; set; }
        public string? BranchName  { get; set; }
        public int?    CounterId   { get; set; }
        public string? CounterName { get; set; }
        public DateTime? ValidFrom  { get; set; }
        public DateTime? ValidUntil { get; set; }
        public bool    IsActive    { get; set; }
    }

    // =====================================================================
    // SHOP INVITE
    // =====================================================================
    public class ShopInviteResponse
    {
        public int      Id             { get; set; }
        public string   Email          { get; set; } = string.Empty;
        public string?  RoleName       { get; set; }
        public string?  BranchName     { get; set; }
        public DateTime ExpiresAt      { get; set; }
        public bool     IsAccepted     { get; set; }
        public DateTime? AcceptedAt   { get; set; }
        public string   InvitedByName  { get; set; } = string.Empty;
        public bool     IsExpired      { get; set; }
        public DateTime CreatedAt      { get; set; }
    }

    // =====================================================================
    // SUBSCRIPTION USAGE
    // =====================================================================
    public class SubscriptionUsageResponse
    {
        public int    Month              { get; set; }
        public int    Year               { get; set; }
        public int    InvoicesCreated    { get; set; }
        public int    MaxInvoices        { get; set; }
        public int    InvoicesPct        { get; set; }
        public int    ProductsCount      { get; set; }
        public int    MaxProducts        { get; set; }
        public int    ProductsPct        { get; set; }
        public int    UsersCount         { get; set; }
        public int    MaxUsers           { get; set; }
        public int    UsersPct           { get; set; }
        public int    BranchesCount      { get; set; }
        public int    MaxBranches        { get; set; }
        public int    BranchesPct        { get; set; }
        public int    ApiCallsCount      { get; set; }
        public string PlanName           { get; set; } = string.Empty;
        public DateTime SubscriptionEnd  { get; set; }
        public int    DaysRemaining      { get; set; }
        public List<UsageAlertResponse> Alerts { get; set; } = new();
    }

    public class UsageAlertResponse
    {
        public int     Id            { get; set; }
        public string  AlertType     { get; set; } = string.Empty;
        public int     ThresholdPct  { get; set; }
        public decimal CurrentValue  { get; set; }
        public decimal LimitValue    { get; set; }
        public bool    IsAcknowledged { get; set; }
        public DateTime CreatedAt    { get; set; }
    }

    // =====================================================================
    // AI INSIGHT
    // =====================================================================
    public class AIInsightResponse
    {
        public int     Id               { get; set; }
        public string  InsightType      { get; set; } = string.Empty;
        public string  Title            { get; set; } = string.Empty;
        public string  Content          { get; set; } = string.Empty;
        public object? Data             { get; set; }
        public object? Recommendation   { get; set; }
        public DateTime PeriodStart     { get; set; }
        public DateTime PeriodEnd       { get; set; }
        public decimal? ConfidenceScore { get; set; }
        public bool    IsRead           { get; set; }
        public bool    IsActedOn        { get; set; }
        public string? RelatedEntityType { get; set; }
        public int?    RelatedEntityId  { get; set; }
        public DateTime GeneratedAt     { get; set; }
    }

    // =====================================================================
    // AI FORECAST
    // =====================================================================
    public class AIForecastResponse
    {
        public int     Id              { get; set; }
        public int     ProductId       { get; set; }
        public string  ProductName     { get; set; } = string.Empty;
        public string? CategoryName    { get; set; }
        public DateTime ForecastDate   { get; set; }
        public string  Granularity     { get; set; } = "Day";
        public decimal ForecastedQty   { get; set; }
        public decimal ActualQty       { get; set; }
        public decimal? ConfidenceLow  { get; set; }
        public decimal? ConfidenceHigh { get; set; }
        public string? ModelVersion    { get; set; }
    }

    // =====================================================================
    // TENANT OVERVIEW (admin dashboard)
    // =====================================================================
    public class TenantOverviewResponse
    {
        public int     TenantId       { get; set; }
        public string  BusinessName   { get; set; } = string.Empty;
        public string? Status         { get; set; }
        public string? PlanName       { get; set; }
        public DateTime? SubscriptionEnd { get; set; }
        public int     TotalBranches  { get; set; }
        public int     TotalUsers     { get; set; }
        public int     TotalProducts  { get; set; }
        public int     InvoicesThisMonth { get; set; }
        public decimal RevenueThisMonth  { get; set; }
        public int     UnreadInsights    { get; set; }
        public List<UsageAlertResponse> ActiveAlerts { get; set; } = new();
    }
}
