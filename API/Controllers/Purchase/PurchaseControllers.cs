using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Models.Entities;
using MasterBilling.API.Services.Interfaces;

namespace MasterBilling.API.Controllers.Purchase
{
    [ApiController] [Route("api/purchases")] [Authorize]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _service;
        public PurchaseController(IPurchaseService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<PurchaseResponse>>>> GetAll([FromQuery] PaginationRequest p) { var (items, total) = await _service.GetAllPurchasesAsync(TenantId, p); return Ok(ApiResponse<List<PurchaseResponse>>.SuccessResponse(items, "Success", total, p.Page, p.PageSize)); }
        [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<PurchaseResponse>>> GetById(int id) { var r = await _service.GetPurchaseByIdAsync(id, TenantId); return r == null ? NotFound(ApiResponse<PurchaseResponse>.FailResponse("Not found")) : Ok(ApiResponse<PurchaseResponse>.SuccessResponse(r)); }
        [HttpPost] public async Task<ActionResult<ApiResponse<PurchaseResponse>>> Create([FromBody] CreatePurchaseRequest r) => Ok(ApiResponse<PurchaseResponse>.SuccessResponse(await _service.CreatePurchaseAsync(r, TenantId), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<PurchaseResponse>>> Update(int id, [FromBody] UpdatePurchaseRequest r) => Ok(ApiResponse<PurchaseResponse>.SuccessResponse(await _service.UpdatePurchaseAsync(id, r, TenantId), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _service.DeletePurchaseAsync(id, TenantId), "Deleted"));
    }

    [ApiController] [Route("api/purchase-orders")] [Authorize]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderService _s;
        public PurchaseOrderController(IPurchaseOrderService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<PurchaseOrder>>>> GetAll() => Ok(ApiResponse<List<PurchaseOrder>>.SuccessResponse(await _s.GetAllAsync(TenantId)));
        [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<PurchaseOrder>>> GetById(int id) { var r = await _s.GetByIdAsync(id); return r == null ? NotFound(ApiResponse<PurchaseOrder>.FailResponse("Not found")) : Ok(ApiResponse<PurchaseOrder>.SuccessResponse(r)); }
        [HttpPost] public async Task<ActionResult<ApiResponse<PurchaseOrder>>> Create([FromBody] CreatePurchaseOrderRequest r) => Ok(ApiResponse<PurchaseOrder>.SuccessResponse(await _s.CreateAsync(r, TenantId), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<PurchaseOrder>>> Update(int id, [FromBody] UpdatePurchaseOrderRequest r) => Ok(ApiResponse<PurchaseOrder>.SuccessResponse(await _s.UpdateAsync(id, r), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _s.DeleteAsync(id), "Deleted"));
    }

    [ApiController] [Route("api/purchase-items")] [Authorize]
    public class PurchaseItemController : ControllerBase
    {
        private readonly IPurchasePaymentService _s;
        public PurchaseItemController(IPurchasePaymentService s) { _s = s; }
        [HttpGet("{purchaseId}")] public async Task<ActionResult<ApiResponse<List<PurchasePayment>>>> GetByPurchase(int purchaseId) => Ok(ApiResponse<List<PurchasePayment>>.SuccessResponse(await _s.GetPaymentsByPurchaseAsync(purchaseId)));
    }

    [ApiController] [Route("api/purchase-payments")] [Authorize]
    public class PurchasePaymentController : ControllerBase
    {
        private readonly IPurchasePaymentService _s;
        public PurchasePaymentController(IPurchasePaymentService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet("{purchaseId}")] public async Task<ActionResult<ApiResponse<List<PurchasePayment>>>> GetByPurchase(int purchaseId) => Ok(ApiResponse<List<PurchasePayment>>.SuccessResponse(await _s.GetPaymentsByPurchaseAsync(purchaseId)));
        [HttpPost] public async Task<ActionResult<ApiResponse<PurchasePayment>>> Create([FromBody] CreatePurchasePaymentRequest r) => Ok(ApiResponse<PurchasePayment>.SuccessResponse(await _s.CreatePaymentAsync(r, TenantId), "Created"));
    }

    [ApiController] [Route("api/purchase-returns")] [Authorize]
    public class PurchaseReturnController : ControllerBase
    {
        private readonly IPurchaseReturnService _s;
        public PurchaseReturnController(IPurchaseReturnService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private static PurchaseReturnResponse Map(PurchaseReturn ret) => new()
        {
            Id = ret.Id, ReturnNumber = ret.ReturnNumber, PurchaseId = ret.PurchaseId, SupplierId = ret.SupplierId,
            Reason = ret.Reason, ReturnDate = ret.ReturnDate, TotalAmount = ret.TotalAmount, Status = ret.Status,
            Items = ret.Items.Select(i => new ReturnItemResponse { ProductId = i.ProductId, Quantity = i.Quantity, UnitPrice = i.UnitPrice, TotalPrice = i.TotalPrice }).ToList()
        };
        [HttpGet] public async Task<ActionResult<ApiResponse<List<PurchaseReturnResponse>>>> GetAll() => Ok(ApiResponse<List<PurchaseReturnResponse>>.SuccessResponse((await _s.GetAllAsync(TenantId)).Select(Map).ToList()));
        [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<PurchaseReturnResponse>>> GetById(int id) { var r = await _s.GetByIdAsync(id); return r == null || r.TenantId != TenantId ? NotFound(ApiResponse<PurchaseReturnResponse>.FailResponse("Not found")) : Ok(ApiResponse<PurchaseReturnResponse>.SuccessResponse(Map(r))); }
        [HttpPost] public async Task<ActionResult<ApiResponse<PurchaseReturnResponse>>> Create([FromBody] CreatePurchaseReturnRequest r) => Ok(ApiResponse<PurchaseReturnResponse>.SuccessResponse(Map(await _s.CreateAsync(r, TenantId)), "Created"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) { var ret = await _s.GetByIdAsync(id); if (ret == null || ret.TenantId != TenantId) return NotFound(ApiResponse<bool>.FailResponse("Not found")); return Ok(ApiResponse<bool>.SuccessResponse(await _s.DeleteAsync(id), "Deleted")); }
    }

    [ApiController] [Route("api/goods-receipts")] [Authorize]
    public class GoodsReceiptController : ControllerBase
    {
        private readonly IGoodsReceiptService _s;
        public GoodsReceiptController(IGoodsReceiptService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<GoodsReceipt>>>> GetAll() => Ok(ApiResponse<List<GoodsReceipt>>.SuccessResponse(await _s.GetAllAsync(TenantId)));
        [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<GoodsReceipt>>> GetById(int id) { var r = await _s.GetByIdAsync(id); return r == null ? NotFound(ApiResponse<GoodsReceipt>.FailResponse("Not found")) : Ok(ApiResponse<GoodsReceipt>.SuccessResponse(r)); }
        [HttpPost] public async Task<ActionResult<ApiResponse<GoodsReceipt>>> Create([FromBody] CreateGoodsReceiptRequest r) => Ok(ApiResponse<GoodsReceipt>.SuccessResponse(await _s.CreateAsync(r, TenantId), "Created"));
    }
}
