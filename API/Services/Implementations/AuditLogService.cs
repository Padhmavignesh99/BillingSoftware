using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingSoftware.API.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _db;
        public AuditLogService(ApplicationDbContext db) { _db = db; }

        public async Task<(List<AuditLogResponse> Items, int Total)> GetAuditLogsAsync(int tenantId, AuditLogFilterRequest filter)
        {
            var query = _db.AuditLogs
                .Include(a => a.User)
                .Where(a => a.TenantId == tenantId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.EntityType))
                query = query.Where(a => a.EntityType == filter.EntityType);
            if (filter.UserId.HasValue)
                query = query.Where(a => a.UserId == filter.UserId);
            if (!string.IsNullOrEmpty(filter.Action))
                query = query.Where(a => a.Action == filter.Action);
            if (filter.From.HasValue)
                query = query.Where(a => a.Timestamp >= filter.From.Value);
            if (filter.To.HasValue)
                query = query.Where(a => a.Timestamp <= filter.To.Value);
            if (!string.IsNullOrEmpty(filter.Search))
                query = query.Where(a => a.EntityType.Contains(filter.Search) || a.Action.Contains(filter.Search));

            var total = await query.CountAsync();
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

            var items = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogResponse
                {
                    Id = a.Id,
                    TenantId = a.TenantId,
                    UserId = a.UserId,
                    UserName = a.User != null ? a.User.Name : null,
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    OldValues = a.OldValues,
                    NewValues = a.NewValues,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    Timestamp = a.Timestamp
                })
                .ToListAsync();

            return (items, total);
        }

        public async Task<List<string>> GetEntityTypesAsync(int tenantId)
        {
            return await _db.AuditLogs
                .Where(a => a.TenantId == tenantId)
                .Select(a => a.EntityType)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();
        }

        public async Task<int> ClearOldLogsAsync(int tenantId, int olderThanDays)
        {
            var cutoff = DateTime.UtcNow.AddDays(-olderThanDays);
            var old = _db.AuditLogs.Where(a => a.TenantId == tenantId && a.Timestamp < cutoff);
            var count = await old.CountAsync();
            _db.AuditLogs.RemoveRange(old);
            await _db.SaveChangesAsync();
            return count;
        }
    }
}

