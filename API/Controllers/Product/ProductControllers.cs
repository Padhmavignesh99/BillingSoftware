using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Models.Entities;
using MasterBilling.API.Services.Interfaces;

namespace MasterBilling.API.Controllers.Product
{
    [ApiController] [Route("api/products")] [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;
        public ProductController(IProductService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        [HttpGet] public async Task<ActionResult<ApiResponse<List<ProductResponse>>>> GetAll([FromQuery] PaginationRequest p) { var (items, total) = await _service.GetAllProductsAsync(TenantId, p); return Ok(ApiResponse<List<ProductResponse>>.SuccessResponse(items, "Success", total, p.Page, p.PageSize)); }
        [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<ProductResponse>>> GetById(int id) { var r = await _service.GetProductByIdAsync(id, TenantId); return r == null ? NotFound(ApiResponse<ProductResponse>.FailResponse("Not found")) : Ok(ApiResponse<ProductResponse>.SuccessResponse(r)); }
        [HttpPost] public async Task<ActionResult<ApiResponse<ProductResponse>>> Create([FromBody] CreateProductRequest r) => Ok(ApiResponse<ProductResponse>.SuccessResponse(await _service.CreateProductAsync(r, TenantId), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<ProductResponse>>> Update(int id, [FromBody] UpdateProductRequest r) => Ok(ApiResponse<ProductResponse>.SuccessResponse(await _service.UpdateProductAsync(id, r, TenantId), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _service.DeleteProductAsync(id, TenantId), "Deleted"));
        [HttpGet("search")] public async Task<ActionResult<ApiResponse<List<ProductResponse>>>> Search([FromQuery] string query) => Ok(ApiResponse<List<ProductResponse>>.SuccessResponse(await _service.SearchProductsAsync(TenantId, query)));
        [HttpGet("barcode/{code}")] public async Task<ActionResult<ApiResponse<ProductResponse>>> GetByBarcode(string code) { var r = await _service.GetProductByBarcodeAsync(TenantId, code); return r == null ? NotFound(ApiResponse<ProductResponse>.FailResponse("Not found")) : Ok(ApiResponse<ProductResponse>.SuccessResponse(r)); }
        [HttpPost("bulk-upload")] public async Task<ActionResult<ApiResponse<List<ProductResponse>>>> BulkUpload([FromBody] BulkProductUploadRequest r) => Ok(ApiResponse<List<ProductResponse>>.SuccessResponse(await _service.BulkUploadAsync(r, TenantId), "Uploaded"));
    }

    [ApiController] [Route("api/product-categories")] [Authorize]
    public class ProductCategoryController : ControllerBase
    {
        private readonly ICategoryService _service;
        public ProductCategoryController(ICategoryService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<CategoryResponse>>>> GetAll() => Ok(ApiResponse<List<CategoryResponse>>.SuccessResponse(await _service.GetAllCategoriesAsync(TenantId)));
        [HttpPost] public async Task<ActionResult<ApiResponse<CategoryResponse>>> Create([FromBody] CreateCategoryRequest r) => Ok(ApiResponse<CategoryResponse>.SuccessResponse(await _service.CreateCategoryAsync(r, TenantId), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<CategoryResponse>>> Update(int id, [FromBody] UpdateCategoryRequest r) => Ok(ApiResponse<CategoryResponse>.SuccessResponse(await _service.UpdateCategoryAsync(id, r), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _service.DeleteCategoryAsync(id), "Deleted"));
    }

    [ApiController] [Route("api/brands")] [Authorize]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _service;
        public BrandController(IBrandService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<Brand>>>> GetAll() => Ok(ApiResponse<List<Brand>>.SuccessResponse(await _service.GetAllBrandsAsync(TenantId)));
        [HttpPost] public async Task<ActionResult<ApiResponse<Brand>>> Create([FromBody] CreateBrandRequest r) => Ok(ApiResponse<Brand>.SuccessResponse(await _service.CreateBrandAsync(r, TenantId), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<Brand>>> Update(int id, [FromBody] CreateBrandRequest r) => Ok(ApiResponse<Brand>.SuccessResponse(await _service.UpdateBrandAsync(id, r), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _service.DeleteBrandAsync(id), "Deleted"));
    }

    [ApiController] [Route("api/units")] [Authorize]
    public class UnitController : ControllerBase
    {
        private readonly IUnitService _service;
        public UnitController(IUnitService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<Unit>>>> GetAll() => Ok(ApiResponse<List<Unit>>.SuccessResponse(await _service.GetAllUnitsAsync(TenantId)));
        [HttpPost] public async Task<ActionResult<ApiResponse<Unit>>> Create([FromBody] CreateUnitRequest r) => Ok(ApiResponse<Unit>.SuccessResponse(await _service.CreateUnitAsync(r, TenantId), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<Unit>>> Update(int id, [FromBody] CreateUnitRequest r) => Ok(ApiResponse<Unit>.SuccessResponse(await _service.UpdateUnitAsync(id, r), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _service.DeleteUnitAsync(id), "Deleted"));
    }

    [ApiController] [Route("api/tax-rates")] [Authorize]
    public class TaxController : ControllerBase
    {
        private readonly ITaxService _service;
        public TaxController(ITaxService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<TaxRateResponse>>>> GetAll() => Ok(ApiResponse<List<TaxRateResponse>>.SuccessResponse(await _service.GetAllTaxRatesAsync(TenantId)));
        [HttpPost] public async Task<ActionResult<ApiResponse<TaxRateResponse>>> Create([FromBody] CreateTaxRateRequest r) => Ok(ApiResponse<TaxRateResponse>.SuccessResponse(await _service.CreateTaxRateAsync(r, TenantId), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<TaxRateResponse>>> Update(int id, [FromBody] UpdateTaxRateRequest r) => Ok(ApiResponse<TaxRateResponse>.SuccessResponse(await _service.UpdateTaxRateAsync(id, r), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _service.DeleteTaxRateAsync(id), "Deleted"));
    }

    [ApiController] [Route("api/discounts")] [Authorize]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _service;
        public DiscountController(IDiscountService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<Discount>>>> GetAll() => Ok(ApiResponse<List<Discount>>.SuccessResponse(await _service.GetAllDiscountsAsync(TenantId)));
        [HttpPost] public async Task<ActionResult<ApiResponse<Discount>>> Create([FromBody] CreateDiscountRequest r) => Ok(ApiResponse<Discount>.SuccessResponse(await _service.CreateDiscountAsync(r, TenantId), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<Discount>>> Update(int id, [FromBody] UpdateDiscountRequest r) => Ok(ApiResponse<Discount>.SuccessResponse(await _service.UpdateDiscountAsync(id, r), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _service.DeleteDiscountAsync(id), "Deleted"));
    }

    [ApiController] [Route("api/barcodes")] [Authorize]
    public class BarcodeController : ControllerBase
    {
        private readonly IBarcodeService _service;
        public BarcodeController(IBarcodeService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpPost("generate/{productId}")] public async Task<ActionResult<ApiResponse<string>>> Generate(int productId) => Ok(ApiResponse<string>.SuccessResponse(await _service.GenerateBarcodeAsync(productId, TenantId)));
        [HttpGet("image/{productId}")] public async Task<IActionResult> GetImage(int productId) { var bytes = await _service.GetBarcodeImageAsync(productId, TenantId); return File(bytes, "image/png"); }
    }
}
