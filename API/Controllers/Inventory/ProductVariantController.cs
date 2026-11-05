using MasterBilling.API.DTOs.Common;
using MasterBilling.API.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MasterBilling.API.Controllers.Inventory
{
    [ApiController]
    [Authorize]
    [Route("api/product-variants")]
    public class ProductVariantController : ControllerBase
    {
        private readonly ProductVariantService _svc;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        public ProductVariantController(ProductVariantService svc) => _svc = svc;

        // ── Attributes ─────────────────────────────────────────────────────────
        [HttpGet("attributes")]
        public async Task<IActionResult> GetAttributes() =>
            Ok(ApiResponse<object>.SuccessResponse(await _svc.GetAttributesAsync(TenantId)));

        [HttpPost("attributes")]
        public async Task<IActionResult> CreateAttribute([FromBody] CreateAttributeReq req)
        {
            var attr = await _svc.CreateAttributeAsync(TenantId, req.Name, req.Description);
            return Ok(ApiResponse<object>.SuccessResponse(attr, "Attribute created"));
        }

        [HttpPost("attributes/{id}/values")]
        public async Task<IActionResult> AddValue(int id, [FromBody] AddAttributeValueReq req)
        {
            var val = await _svc.AddAttributeValueAsync(TenantId, id, req.Value, req.ColorHex);
            return Ok(ApiResponse<object>.SuccessResponse(val, "Value added"));
        }

        // ── Variants ───────────────────────────────────────────────────────────
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetVariants(int productId) =>
            Ok(ApiResponse<object>.SuccessResponse(await _svc.GetVariantsAsync(productId, TenantId)));

        [HttpPost("product/{productId}")]
        public async Task<IActionResult> CreateVariant(int productId, [FromBody] CreateVariantReq req)
        {
            var v = await _svc.CreateVariantAsync(TenantId, productId,
                req.SKU, req.SellingPrice, req.PurchasePrice, req.MRP, req.AttributeValueIds);
            return Ok(ApiResponse<object>.SuccessResponse(v, "Variant created"));
        }

        [HttpPatch("{variantId}/stock")]
        public async Task<IActionResult> UpdateStock(int variantId, [FromBody] UpdateStockReq req)
        {
            var ok = await _svc.UpdateVariantStockAsync(variantId, TenantId, req.Stock);
            return ok
                ? Ok(ApiResponse<object>.SuccessResponse(null, "Stock updated"))
                : NotFound(ApiResponse<object>.FailResponse("Variant not found"));
        }

        [HttpDelete("{variantId}")]
        public async Task<IActionResult> DeleteVariant(int variantId)
        {
            var ok = await _svc.DeleteVariantAsync(variantId, TenantId);
            return ok
                ? Ok(ApiResponse<object>.SuccessResponse(null, "Variant deleted"))
                : NotFound(ApiResponse<object>.FailResponse("Variant not found"));
        }
    }

    // ── Inline request DTOs ────────────────────────────────────────────────────
    public record CreateAttributeReq(string Name, string? Description);
    public record AddAttributeValueReq(string Value, string? ColorHex);
    public record CreateVariantReq(string SKU, decimal? SellingPrice, decimal? PurchasePrice, decimal? MRP, List<int> AttributeValueIds);
    public record UpdateStockReq(int Stock);
}

