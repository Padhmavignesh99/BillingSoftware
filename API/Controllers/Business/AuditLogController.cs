using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MasterBilling.API.DTOs.Common;
using MasterBilling.API.Models.Entities;
using MasterBilling.API.Services.Interfaces;

namespace MasterBilling.API.Controllers.Business
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
        public async Task<ActionResult<ApiResponse<List<AuditLog>>>> GetAll([FromQuery] PaginationRequest p)
        {
            var items = await _s.GetLogsAsync(TenantId, p);
            return Ok(ApiResponse<List<AuditLog>>.SuccessResponse(items));
        }

        [HttpGet("entity-types")]
        public ActionResult<ApiResponse<List<string>>> GetEntityTypes()
        {
            // Return common entity types — actual data filtered on frontend
            var types = new List<string> { "Invoice", "Product", "Customer", "Supplier", "Purchase", "Expense", "Payment", "StockAdjustment" };
            return Ok(ApiResponse<List<string>>.SuccessResponse(types));
        }

        [HttpDelete("clear")]
        public ActionResult<ApiResponse<string>> Clear([FromQuery] int olderThanDays = 90)
        {
            // Log clearing is admin-only; placeholder response
            return Ok(ApiResponse<string>.SuccessResponse("Clear not implemented in base service", "Done"));
        }
    }
}
