using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Services.Interfaces;

namespace BillingSoftware.API.Controllers.Sales
{
    [ApiController]
    [Route("api/delivery-challans")]
    [Authorize]
    public class DeliveryChallanController : ControllerBase
    {
        private readonly IDeliveryChallanService _s;
        public DeliveryChallanController(IDeliveryChallanService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<DeliveryChallanResponse>>>> GetAll([FromQuery] PaginationRequest p)
        {
            var (items, total) = await _s.GetAllAsync(TenantId, p);
            return Ok(ApiResponse<List<DeliveryChallanResponse>>.SuccessResponse(items, "Success", total, p.Page, p.PageSize));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DeliveryChallanResponse>>> GetById(int id)
        {
            var r = await _s.GetByIdAsync(id, TenantId);
            return r == null ? NotFound(ApiResponse<DeliveryChallanResponse>.FailResponse("Not found")) : Ok(ApiResponse<DeliveryChallanResponse>.SuccessResponse(r));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<DeliveryChallanResponse>>> Create([FromBody] CreateDeliveryChallanRequest r)
            => Ok(ApiResponse<DeliveryChallanResponse>.SuccessResponse(await _s.CreateAsync(r, TenantId), "Challan created"));

        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApiResponse<DeliveryChallanResponse>>> UpdateStatus(int id, [FromBody] UpdateChallanStatusRequest r)
            => Ok(ApiResponse<DeliveryChallanResponse>.SuccessResponse(await _s.UpdateStatusAsync(id, r.Status, TenantId), "Status updated"));

        [HttpPost("{id}/convert-to-invoice")]
        public async Task<ActionResult<ApiResponse<InvoiceResponse>>> ConvertToInvoice(int id)
            => Ok(ApiResponse<InvoiceResponse>.SuccessResponse(await _s.ConvertToInvoiceAsync(id, TenantId), "Converted to invoice"));

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
            => Ok(ApiResponse<bool>.SuccessResponse(await _s.DeleteAsync(id, TenantId), "Deleted"));

        [HttpGet("next-number")]
        public async Task<ActionResult<ApiResponse<string>>> GetNextNumber()
            => Ok(ApiResponse<string>.SuccessResponse(await _s.GetNextChallanNumberAsync(TenantId)));
    }

    [ApiController]
    [Route("api/recurring-invoices")]
    [Authorize]
    public class RecurringInvoiceController : ControllerBase
    {
        private readonly IRecurringInvoiceService _s;
        public RecurringInvoiceController(IRecurringInvoiceService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<RecurringInvoiceResponse>>>> GetAll([FromQuery] PaginationRequest p)
        {
            var (items, total) = await _s.GetAllAsync(TenantId, p);
            return Ok(ApiResponse<List<RecurringInvoiceResponse>>.SuccessResponse(items, "Success", total, p.Page, p.PageSize));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RecurringInvoiceResponse>>> GetById(int id)
        {
            var r = await _s.GetByIdAsync(id, TenantId);
            return r == null ? NotFound(ApiResponse<RecurringInvoiceResponse>.FailResponse("Not found")) : Ok(ApiResponse<RecurringInvoiceResponse>.SuccessResponse(r));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<RecurringInvoiceResponse>>> Create([FromBody] CreateRecurringInvoiceRequest r)
            => Ok(ApiResponse<RecurringInvoiceResponse>.SuccessResponse(await _s.CreateAsync(r, TenantId), "Created"));

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<RecurringInvoiceResponse>>> Update(int id, [FromBody] CreateRecurringInvoiceRequest r)
            => Ok(ApiResponse<RecurringInvoiceResponse>.SuccessResponse(await _s.UpdateAsync(id, r, TenantId), "Updated"));

        [HttpPost("{id}/toggle-status")]
        public async Task<ActionResult<ApiResponse<RecurringInvoiceResponse>>> ToggleStatus(int id)
            => Ok(ApiResponse<RecurringInvoiceResponse>.SuccessResponse(await _s.ToggleStatusAsync(id, TenantId), "Status toggled"));

        [HttpPost("{id}/generate-now")]
        public async Task<ActionResult<ApiResponse<InvoiceResponse>>> GenerateNow(int id)
            => Ok(ApiResponse<InvoiceResponse>.SuccessResponse(await _s.GenerateNowAsync(id, TenantId), "Invoice generated"));

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
            => Ok(ApiResponse<bool>.SuccessResponse(await _s.DeleteAsync(id, TenantId), "Deleted"));
    }
}

