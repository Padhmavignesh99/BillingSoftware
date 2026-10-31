using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.Services.Interfaces;

namespace BillingSoftware.API.Controllers.Inventory
{
    [ApiController]
    [Route("api/stock-take")]
    [Authorize]
    public class StockTakeController : ControllerBase
    {
        private readonly IStockTakeService _s;
        public StockTakeController(IStockTakeService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<StockTakeSessionResponse>>>> GetAll([FromQuery] PaginationRequest p)
        {
            var (items, total) = await _s.GetSessionsAsync(TenantId, p);
            return Ok(ApiResponse<List<StockTakeSessionResponse>>.SuccessResponse(items, "Success", total, p.Page, p.PageSize));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<StockTakeSessionResponse>>> GetById(int id)
        {
            var r = await _s.GetSessionByIdAsync(id, TenantId);
            return r == null ? NotFound(ApiResponse<StockTakeSessionResponse>.FailResponse("Not found")) : Ok(ApiResponse<StockTakeSessionResponse>.SuccessResponse(r));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<StockTakeSessionResponse>>> Create([FromBody] CreateStockTakeRequest r)
            => Ok(ApiResponse<StockTakeSessionResponse>.SuccessResponse(await _s.CreateSessionAsync(r, TenantId), "Stock take session created"));

        [HttpPut("{id}/bulk-update")]
        public async Task<ActionResult<ApiResponse<bool>>> BulkUpdate(int id, [FromBody] List<UpdateStockTakeItemRequest> items)
        {
            await _s.BulkUpdateItemsAsync(id, items, TenantId);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Updated"));
        }

        [HttpPost("{id}/complete")]
        public async Task<ActionResult<ApiResponse<StockTakeSessionResponse>>> Complete(int id)
            => Ok(ApiResponse<StockTakeSessionResponse>.SuccessResponse(await _s.CompleteSessionAsync(id, TenantId), "Session completed"));

        [HttpPost("{id}/apply-adjustments")]
        public async Task<ActionResult<ApiResponse<int>>> ApplyAdjustments(int id)
            => Ok(ApiResponse<int>.SuccessResponse(await _s.ApplyAdjustmentsAsync(id, TenantId), "Adjustments applied"));

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
            => Ok(ApiResponse<bool>.SuccessResponse(await _s.DeleteSessionAsync(id, TenantId), "Deleted"));
    }

    [ApiController]
    [Route("api/reorder")]
    [Authorize]
    public class ReorderController : ControllerBase
    {
        private readonly IReorderService _s;
        public ReorderController(IReorderService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        [HttpGet("suggestions")]
        public async Task<ActionResult<ApiResponse<List<ReorderSuggestion>>>> GetSuggestions()
        {
            var items = await _s.GetReorderSuggestionsAsync(TenantId);
            return Ok(ApiResponse<List<ReorderSuggestion>>.SuccessResponse(items));
        }

        [HttpPost("create-purchase-order")]
        public async Task<ActionResult<ApiResponse<object>>> CreatePurchaseOrder([FromBody] List<int> productIds)
        {
            var result = await _s.CreatePurchaseOrderFromSuggestionsAsync(productIds, TenantId);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Purchase orders created"));
        }
    }
}

