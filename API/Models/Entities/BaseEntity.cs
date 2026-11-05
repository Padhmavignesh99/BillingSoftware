namespace MasterBilling.API.Models.Entities
{
    /// <summary>
    /// Base entity with common audit fields for all entities
    /// </summary>
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// Tenant-aware base entity — most entities inherit from this
    /// </summary>
    public abstract class TenantEntity : BaseEntity
    {
        public int TenantId { get; set; }
        public Business? Tenant { get; set; }
    }
}
