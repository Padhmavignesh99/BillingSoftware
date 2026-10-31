using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.Services.Interfaces;

namespace BillingSoftware.API.Controllers.Business
{
    [ApiController]
    [Route("api/audit-logs")]
    [Authorize]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _s;
        public AuditLogController(IAuditLogService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<AuditLogResponse>>>> GetAll([FromQuery] AuditLogFilterRequest filter)
        {
            var (items, total) = await _s.GetAuditLogsAsync(TenantId, filter);
            return Ok(ApiResponse<List<AuditLogResponse>>.SuccessResponse(items, "Success", total, filter.Page, filter.PageSize));
        }

        [HttpGet("entity-types")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetEntityTypes()
        {
            var types = await _s.GetEntityTypesAsync(TenantId);
            return Ok(ApiResponse<List<string>>.SuccessResponse(types));
        }

        [HttpDelete("clear")]
        public async Task<ActionResult<ApiResponse<int>>> Clear([FromQuery] int olderThanDays = 90)
        {
            var count = await _s.ClearOldLogsAsync(TenantId, olderThanDays);
            return Ok(ApiResponse<int>.SuccessResponse(count, $"Cleared {count} log entries"));
        }
    }
}

