using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.Services.Interfaces;

namespace BillingSoftware.API.Controllers.Business
{
    [ApiController]
    [Route("api/export")]
    [Authorize]
    public class ExportController : ControllerBase
    {
        private readonly IExportImportService _s;
        public ExportController(IExportImportService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        [HttpGet("products")]
        public async Task<IActionResult> ExportProducts()
        {
            var csv = await _s.ExportProductsCsvAsync(TenantId);
            return File(csv, "text/csv", $"products-{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet("customers")]
        public async Task<IActionResult> ExportCustomers()
        {
            var csv = await _s.ExportCustomersCsvAsync(TenantId);
            return File(csv, "text/csv", $"customers-{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet("suppliers")]
        public async Task<IActionResult> ExportSuppliers()
        {
            var csv = await _s.ExportSuppliersCsvAsync(TenantId);
            return File(csv, "text/csv", $"suppliers-{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet("invoices")]
        public async Task<IActionResult> ExportInvoices([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var csv = await _s.ExportInvoicesCsvAsync(TenantId, from, to);
            return File(csv, "text/csv", $"invoices-{DateTime.Now:yyyyMMdd}.csv");
        }
    }

    [ApiController]
    [Route("api/import")]
    [Authorize]
    public class ImportController : ControllerBase
    {
        private readonly IExportImportService _s;
        public ImportController(IExportImportService s) { _s = s; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        [HttpPost("products")]
        public async Task<ActionResult<ApiResponse<ImportResult>>> ImportProducts(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<ImportResult>.FailResponse("No file uploaded"));
            var result = await _s.ImportProductsCsvAsync(file.OpenReadStream(), TenantId);
            return Ok(ApiResponse<ImportResult>.SuccessResponse(result, $"Import complete: {result.Success} success, {result.Failed} failed"));
        }

        [HttpPost("customers")]
        public async Task<ActionResult<ApiResponse<ImportResult>>> ImportCustomers(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<ImportResult>.FailResponse("No file uploaded"));
            var result = await _s.ImportCustomersCsvAsync(file.OpenReadStream(), TenantId);
            return Ok(ApiResponse<ImportResult>.SuccessResponse(result, $"Import complete: {result.Success} success, {result.Failed} failed"));
        }

        [HttpGet("template/products")]
        public async Task<IActionResult> GetProductsTemplate()
        {
            var csv = await _s.GetProductsTemplateCsvAsync();
            return File(csv, "text/csv", "products-template.csv");
        }

        [HttpGet("template/customers")]
        public async Task<IActionResult> GetCustomersTemplate()
        {
            var csv = await _s.GetCustomersTemplateCsvAsync();
            return File(csv, "text/csv", "customers-template.csv");
        }
    }
}

