using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Services.Interfaces;

namespace BillingSoftware.API.Controllers.Inventory
{
    [ApiController] [Route("api/inventory")] [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _service;
        public InventoryController(IInventoryService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<InventoryResponse>>>> GetAll() => Ok(ApiResponse<List<InventoryResponse>>.SuccessResponse(await _service.GetAllInventoryAsync(TenantId)));
        [HttpPut("{productId}/adjust")] public async Task<ActionResult<ApiResponse<InventoryResponse>>> Adjust(int productId, [FromBody] AdjustStockRequest r) => Ok(ApiResponse<InventoryResponse>.SuccessResponse(await _service.AdjustStockAsync(productId, r, TenantId), "Stock adjusted"));
        [HttpGet("low-stock")] public async Task<ActionResult<ApiResponse<List<ProductResponse>>>> GetLowStock() => Ok(ApiResponse<List<ProductResponse>>.SuccessResponse(await _service.GetLowStockProductsAsync(TenantId)));
        [HttpGet("stock-history/{productId}")] public async Task<ActionResult<ApiResponse<List<StockHistoryResponse>>>> GetHistory(int productId) => Ok(ApiResponse<List<StockHistoryResponse>>.SuccessResponse(await _service.GetStockHistoryAsync(productId, TenantId)));
    }

    [ApiController] [Route("api/stock")] [Authorize]
    public class StockController : ControllerBase
    {
        private readonly IStockService _service;
        public StockController(IStockService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet("transactions")] public async Task<ActionResult<ApiResponse<List<StockTransaction>>>> GetTransactions([FromQuery] PaginationRequest p) => Ok(ApiResponse<List<StockTransaction>>.SuccessResponse(await _service.GetStockTransactionsAsync(TenantId, p)));
    }

    [ApiController] [Route("api/stock-transactions")] [Authorize]
    public class StockTransactionController : ControllerBase
    {
        private readonly IStockService _service;
        public StockTransactionController(IStockService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<StockTransaction>>>> GetAll([FromQuery] PaginationRequest p) => Ok(ApiResponse<List<StockTransaction>>.SuccessResponse(await _service.GetStockTransactionsAsync(TenantId, p)));
    }

    [ApiController] [Route("api/stock-adjustments")] [Authorize]
    public class StockAdjustmentController : ControllerBase
    {
        private readonly IStockService _service;
        public StockAdjustmentController(IStockService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<StockAdjustment>>>> GetAll() => Ok(ApiResponse<List<StockAdjustment>>.SuccessResponse(await _service.GetAdjustmentsAsync(TenantId)));
        [HttpPost] public async Task<ActionResult<ApiResponse<StockAdjustment>>> Create([FromBody] CreateStockAdjustmentRequest r) => Ok(ApiResponse<StockAdjustment>.SuccessResponse(await _service.CreateAdjustmentAsync(r, TenantId), "Created"));
    }

    [ApiController] [Route("api/stock-transfers")] [Authorize]
    public class StockTransferController : ControllerBase
    {
        private readonly IStockService _service;
        public StockTransferController(IStockService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<StockTransfer>>>> GetAll() => Ok(ApiResponse<List<StockTransfer>>.SuccessResponse(await _service.GetTransfersAsync(TenantId)));
        [HttpPost] public async Task<ActionResult<ApiResponse<StockTransfer>>> Create([FromBody] CreateStockTransferRequest r) => Ok(ApiResponse<StockTransfer>.SuccessResponse(await _service.CreateTransferAsync(r, TenantId), "Created"));
        [HttpPut("{id}/status")] public async Task<ActionResult<ApiResponse<StockTransfer>>> UpdateStatus(int id, [FromBody] string status) => Ok(ApiResponse<StockTransfer>.SuccessResponse(await _service.UpdateTransferStatusAsync(id, status), "Updated"));
    }

    [ApiController] [Route("api/warehouses")] [Authorize]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _service;
        public WarehouseController(IWarehouseService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<Warehouse>>>> GetAll() => Ok(ApiResponse<List<Warehouse>>.SuccessResponse(await _service.GetAllWarehousesAsync(TenantId)));
        [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<Warehouse>>> GetById(int id) { var r = await _service.GetWarehouseByIdAsync(id); return r == null ? NotFound(ApiResponse<Warehouse>.FailResponse("Not found")) : Ok(ApiResponse<Warehouse>.SuccessResponse(r)); }
        [HttpPost] public async Task<ActionResult<ApiResponse<Warehouse>>> Create([FromBody] CreateWarehouseRequest r) => Ok(ApiResponse<Warehouse>.SuccessResponse(await _service.CreateWarehouseAsync(r, TenantId), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<Warehouse>>> Update(int id, [FromBody] UpdateWarehouseRequest r) => Ok(ApiResponse<Warehouse>.SuccessResponse(await _service.UpdateWarehouseAsync(id, r), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _service.DeleteWarehouseAsync(id), "Deleted"));
    }

    [ApiController] [Route("api/low-stock")] [Authorize]
    public class LowStockController : ControllerBase
    {
        private readonly IInventoryService _service;
        public LowStockController(IInventoryService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<ProductResponse>>>> GetLowStock() => Ok(ApiResponse<List<ProductResponse>>.SuccessResponse(await _service.GetLowStockProductsAsync(TenantId)));
    }
}
