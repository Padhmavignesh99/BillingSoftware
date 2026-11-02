using BillingSoftware.API.DTOs.Common;

namespace BillingSoftware.API.DTOs.Requests
{
    public class AuditLogFilterRequest : PaginationRequest
    {
        public string? EntityType { get; set; }
        public int? UserId { get; set; }
        public string? Action { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Search { get; set; }
    }

    public class AuditLogResponse
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
