using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Services.Interfaces;

// Wrapper DTO required by Swashbuckle to correctly generate multipart/form-data schema
public class UploadLogoRequest { public IFormFile File { get; set; } = null!; }

namespace BillingSoftware.API.Controllers.Business
{
    [ApiController] [Route("api/[controller]")] [Authorize]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _service;
        public BusinessController(IBusinessService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<BusinessResponse>>> GetById(int id) { var r = await _service.GetBusinessByIdAsync(id); return r == null ? NotFound(ApiResponse<BusinessResponse>.FailResponse("Not found")) : Ok(ApiResponse<BusinessResponse>.SuccessResponse(r)); }
        [HttpGet] public async Task<ActionResult<ApiResponse<BusinessResponse>>> GetCurrent() { var r = await _service.GetBusinessByIdAsync(TenantId); return r == null ? NotFound(ApiResponse<BusinessResponse>.FailResponse("Not found")) : Ok(ApiResponse<BusinessResponse>.SuccessResponse(r)); }
        [HttpPost] public async Task<ActionResult<ApiResponse<BusinessResponse>>> Create([FromBody] CreateBusinessRequest request) => Ok(ApiResponse<BusinessResponse>.SuccessResponse(await _service.CreateBusinessAsync(request), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<BusinessResponse>>> Update(int id, [FromBody] UpdateBusinessRequest request) => Ok(ApiResponse<BusinessResponse>.SuccessResponse(await _service.UpdateBusinessAsync(id, request), "Updated"));
        [HttpPost("logo-upload")] [Consumes("multipart/form-data")] public async Task<ActionResult<ApiResponse<bool>>> UploadLogo([FromForm] UploadLogoRequest request) { var file = request.File; var path = Path.Combine("uploads", "logos", $"{TenantId}_{file.FileName}"); Directory.CreateDirectory(Path.GetDirectoryName(path)!); using (var stream = new FileStream(path, FileMode.Create)) await file.CopyToAsync(stream); return Ok(ApiResponse<bool>.SuccessResponse(await _service.UploadLogoAsync(TenantId, $"/uploads/logos/{TenantId}_{file.FileName}"), "Logo uploaded")); }
    }

    [ApiController] [Route("api/business-types")] [Authorize]
    public class BusinessTypeController : ControllerBase
    {
        private readonly IBusinessTypeService _service;
        public BusinessTypeController(IBusinessTypeService service) { _service = service; }
        [HttpGet] public async Task<ActionResult<ApiResponse<List<BusinessType>>>> GetAll() => Ok(ApiResponse<List<BusinessType>>.SuccessResponse(await _service.GetAllAsync()));
        [HttpPost] public async Task<ActionResult<ApiResponse<BusinessType>>> Create([FromBody] CreateBusinessTypeRequest r) => Ok(ApiResponse<BusinessType>.SuccessResponse(await _service.CreateAsync(r), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<BusinessType>>> Update(int id, [FromBody] CreateBusinessTypeRequest r) => Ok(ApiResponse<BusinessType>.SuccessResponse(await _service.UpdateAsync(id, r), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _service.DeleteAsync(id), "Deleted"));
    }

    [ApiController] [Route("api/[controller]")] [Authorize]
    public class BranchController : ControllerBase
    {
        private readonly IBranchService _service;
        public BranchController(IBranchService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<List<BranchResponse>>>> GetAll() => Ok(ApiResponse<List<BranchResponse>>.SuccessResponse(await _service.GetAllBranchesAsync(TenantId)));
        [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<BranchResponse>>> GetById(int id) { var r = await _service.GetBranchByIdAsync(id); return r == null ? NotFound(ApiResponse<BranchResponse>.FailResponse("Not found")) : Ok(ApiResponse<BranchResponse>.SuccessResponse(r)); }
        [HttpPost] public async Task<ActionResult<ApiResponse<BranchResponse>>> Create([FromBody] CreateBranchRequest r) => Ok(ApiResponse<BranchResponse>.SuccessResponse(await _service.CreateBranchAsync(r, TenantId), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<BranchResponse>>> Update(int id, [FromBody] UpdateBranchRequest r) => Ok(ApiResponse<BranchResponse>.SuccessResponse(await _service.UpdateBranchAsync(id, r), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _service.DeleteBranchAsync(id), "Deleted"));
    }

    [ApiController] [Route("api/business-settings")] [Authorize]
    public class BusinessSettingsController : ControllerBase
    {
        private readonly IBusinessSettingsService _service;
        public BusinessSettingsController(IBusinessSettingsService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<BusinessSettings>>> Get() => Ok(ApiResponse<BusinessSettings>.SuccessResponse(await _service.GetSettingsAsync(TenantId)));
        [HttpPut] public async Task<ActionResult<ApiResponse<BusinessSettings>>> Update([FromBody] UpdateBusinessSettingsRequest r) => Ok(ApiResponse<BusinessSettings>.SuccessResponse(await _service.UpdateSettingsAsync(TenantId, r), "Updated"));
    }

    [ApiController] [Route("api/invoice-settings")] [Authorize]
    public class InvoiceSettingsController : ControllerBase
    {
        private readonly IBusinessSettingsService _service;
        public InvoiceSettingsController(IBusinessSettingsService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        [HttpGet] public async Task<ActionResult<ApiResponse<InvoiceSettings>>> Get() => Ok(ApiResponse<InvoiceSettings>.SuccessResponse(await _service.GetInvoiceSettingsAsync(TenantId)));
        [HttpPut] public async Task<ActionResult<ApiResponse<InvoiceSettings>>> Update([FromBody] UpdateInvoiceSettingsRequest r) => Ok(ApiResponse<InvoiceSettings>.SuccessResponse(await _service.UpdateInvoiceSettingsAsync(TenantId, r), "Updated"));
    }
}
