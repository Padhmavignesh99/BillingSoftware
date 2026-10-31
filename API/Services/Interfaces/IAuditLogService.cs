using BillingSoftware.API.DTOs.Requests;

namespace BillingSoftware.API.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<(List<AuditLogResponse> Items, int Total)> GetAuditLogsAsync(int tenantId, AuditLogFilterRequest filter);
        Task<List<string>> GetEntityTypesAsync(int tenantId);
        Task<int> ClearOldLogsAsync(int tenantId, int olderThanDays);
    }
}

