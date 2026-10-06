namespace BillingSoftware.API.Services.Interfaces
{
    /// <summary>
    /// Resolves the current tenant ID from the authenticated user's JWT claims.
    /// Inject this wherever you need the calling tenant's identity.
    /// SuperAdmin users have TenantId = null (unrestricted access).
    /// </summary>
    public interface ITenantContext
    {
        /// <summary>Current tenant ID. Null means SuperAdmin (cross-tenant access).</summary>
        int? TenantId { get; }

        /// <summary>True if the current user is a SuperAdmin.</summary>
        bool IsSuperAdmin { get; }

        /// <summary>Returns TenantId for regular users; throws for unauthenticated requests.</summary>
        int RequireTenantId();
    }
}
