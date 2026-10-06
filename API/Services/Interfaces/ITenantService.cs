using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;

namespace BillingSoftware.API.Services.Interfaces
{
    public interface ITenantService
    {
        /// <summary>Get all registered tenants/businesses (SuperAdmin only).</summary>
        Task<List<TenantResponse>> GetAllTenantsAsync();

        /// <summary>Get a specific tenant by ID.</summary>
        Task<TenantResponse?> GetTenantByIdAsync(int tenantId);

        /// <summary>Get aggregated stats for a tenant.</summary>
        Task<TenantStatsResponse> GetTenantStatsAsync(int tenantId);

        /// <summary>Suspend a tenant — disables their access.</summary>
        Task<bool> SuspendTenantAsync(int tenantId);

        /// <summary>Re-activate a suspended tenant.</summary>
        Task<bool> ActivateTenantAsync(int tenantId);

        /// <summary>Get all users belonging to a specific tenant.</summary>
        Task<List<UserResponse>> GetTenantUsersAsync(int tenantId);
    }
}
