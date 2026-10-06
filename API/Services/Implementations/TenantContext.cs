using BillingSoftware.API.Models.Enums;
using BillingSoftware.API.Services.Interfaces;
using System.Security.Claims;

namespace BillingSoftware.API.Services.Implementations
{
    /// <summary>
    /// Scoped service — resolves the current tenant from the HTTP request's JWT claims.
    /// Registered once per request via DI.
    /// </summary>
    public class TenantContext : ITenantContext
    {
        public int? TenantId { get; }
        public bool IsSuperAdmin { get; }

        public TenantContext(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return;

            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            IsSuperAdmin = role == nameof(UserRole.SuperAdmin);

            var tenantClaim = user.FindFirst("TenantId")?.Value;
            if (int.TryParse(tenantClaim, out var id) && id > 0)
                TenantId = id;
        }

        /// <summary>
        /// Returns the TenantId for normal users.
        /// SuperAdmins should not call this — use TenantId directly and handle null.
        /// </summary>
        public int RequireTenantId()
        {
            if (TenantId == null)
                throw new UnauthorizedAccessException("No tenant context for this request.");
            return TenantId.Value;
        }
    }
}
