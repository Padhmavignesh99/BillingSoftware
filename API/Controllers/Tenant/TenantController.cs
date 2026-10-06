using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Services.Interfaces;

namespace BillingSoftware.API.Controllers.Tenant
{
    /// <summary>
    /// SuperAdmin-only endpoints for managing all tenants (businesses/customers).
    /// Vignesh's business = TenantId 1, Murugan's = TenantId 2, etc.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class TenantController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>List all registered tenants (customers who bought the software).</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TenantResponse>>>> GetAll()
        {
            var tenants = await _tenantService.GetAllTenantsAsync();
            return Ok(ApiResponse<List<TenantResponse>>.SuccessResponse(tenants,
                $"{tenants.Count} tenant(s) found"));
        }

        /// <summary>Get full details of a specific tenant by their business ID.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<TenantResponse>>> GetById(int id)
        {
            var tenant = await _tenantService.GetTenantByIdAsync(id);
            return tenant == null
                ? NotFound(ApiResponse<TenantResponse>.FailResponse($"Tenant {id} not found"))
                : Ok(ApiResponse<TenantResponse>.SuccessResponse(tenant));
        }

        /// <summary>Get aggregated stats for a tenant — users, products, invoices, revenue.</summary>
        [HttpGet("{id:int}/stats")]
        public async Task<ActionResult<ApiResponse<TenantStatsResponse>>> GetStats(int id)
        {
            var stats = await _tenantService.GetTenantStatsAsync(id);
            return Ok(ApiResponse<TenantStatsResponse>.SuccessResponse(stats));
        }

        /// <summary>Get all users belonging to a specific tenant.</summary>
        [HttpGet("{id:int}/users")]
        public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetUsers(int id)
        {
            var users = await _tenantService.GetTenantUsersAsync(id);
            return Ok(ApiResponse<List<UserResponse>>.SuccessResponse(users,
                $"{users.Count} user(s) in tenant {id}"));
        }

        /// <summary>Suspend a tenant — sets BusinessStatus to Suspended.</summary>
        [HttpPut("{id:int}/suspend")]
        public async Task<ActionResult<ApiResponse<bool>>> Suspend(int id)
        {
            var result = await _tenantService.SuspendTenantAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, $"Tenant {id} suspended"));
        }

        /// <summary>Re-activate a suspended tenant — sets BusinessStatus to Active.</summary>
        [HttpPut("{id:int}/activate")]
        public async Task<ActionResult<ApiResponse<bool>>> Activate(int id)
        {
            var result = await _tenantService.ActivateTenantAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, $"Tenant {id} activated"));
        }
    }
}
