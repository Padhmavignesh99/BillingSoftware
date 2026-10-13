using System.ComponentModel.DataAnnotations;

namespace BillingSoftware.API.DTOs.Requests
{
    // =====================================================================
    // WAREHOUSE REQUESTS
    // =====================================================================
    // CreateWarehouseRequest / UpdateWarehouseRequest live in TransactionRequestDtos.cs


    public class CreateCounterRequest
    {
        [Required, MaxLength(100)]
        public string  Name            { get; set; } = string.Empty;
        [MaxLength(20)]
        public string? Code            { get; set; }
        [Required]
        public int     BranchId        { get; set; }
        public string? Description     { get; set; }
        public bool    IsDefault       { get; set; } = false;
        public int?    AssignedUserId  { get; set; }
        public string? DeviceId        { get; set; }
        public string? IpAddress       { get; set; }
    }

    public class UpdateCounterRequest : CreateCounterRequest { }

    public class AssignCounterUserRequest
    {
        [Required]
        public int UserId { get; set; }
    }

    // =====================================================================
    // EMPLOYEE ROLE REQUESTS
    // =====================================================================
    public class CreateEmployeeRoleRequest
    {
        [Required, MaxLength(80)]
        public string  Name        { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color       { get; set; }
        /// <summary>Permission keys to grant (e.g. ["Invoices.Create","Invoices.View"])</summary>
        public List<string> PermissionKeys { get; set; } = new();
    }

    public class UpdateEmployeeRoleRequest : CreateEmployeeRoleRequest { }

    public class SetRolePermissionsRequest
    {
        [Required]
        public List<string> PermissionKeys { get; set; } = new();
    }

    // =====================================================================
    // USER ROLE ASSIGNMENT
    // =====================================================================
    public class AssignUserRoleRequest
    {
        [Required]
        public int     UserId     { get; set; }
        [Required]
        public int     RoleId     { get; set; }
        public int?    BranchId   { get; set; }
        public int?    CounterId  { get; set; }
        public DateTime? ValidFrom  { get; set; }
        public DateTime? ValidUntil { get; set; }
    }

    public class RevokeUserRoleRequest
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int RoleId { get; set; }
    }

    // =====================================================================
    // SHOP INVITE REQUESTS
    // =====================================================================
    public class SendShopInviteRequest
    {
        [Required, EmailAddress]
        public string Email    { get; set; } = string.Empty;
        public int?   RoleId   { get; set; }
        public int?   BranchId { get; set; }
    }

    public class AcceptShopInviteRequest
    {
        [Required]
        public string Token    { get; set; } = string.Empty;
        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string FullName { get; set; } = string.Empty;
    }

    // =====================================================================
    // AI INSIGHT REQUEST
    // =====================================================================
    public class GenerateInsightRequest
    {
        /// <summary>SalesSummary | DemandForecast | ReorderSuggestion | AnomalyAlert | CustomerInsight</summary>
        [Required]
        public string InsightType  { get; set; } = "SalesSummary";
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd   { get; set; }
        /// <summary>Day | Week | Month</summary>
        public string  Granularity { get; set; } = "Day";
        public int?    ProductId   { get; set; }
        public int?    CategoryId  { get; set; }
        public int?    BranchId    { get; set; }
    }

    public class MarkInsightReadRequest
    {
        public bool IsRead    { get; set; } = true;
        public bool IsActedOn { get; set; } = false;
    }
}
