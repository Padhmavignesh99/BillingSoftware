using BillingSoftware.API.Models.Enums;

namespace BillingSoftware.API.Models.Entities
{
    // ==================== USER THEME SETTINGS ====================
    /// <summary>
    /// Stores per-user UI theme preferences.
    /// Each user has at most one record; it is created on first save and updated thereafter.
    /// TenantId is kept for multi-tenant isolation even though the row is user-specific.
    /// </summary>
    public class UserThemeSettings : BaseEntity
    {
        // ── Identity ─────────────────────────────────────────────────────────────
        public int UserId { get; set; }
        public User? User { get; set; }
        public int TenantId { get; set; }
        public Business? Tenant { get; set; }

        // ── Color Mode ────────────────────────────────────────────────────────────
        /// <summary>Light | Dark | System</summary>
        public string ColorMode { get; set; } = "Light";

        // ── Primary Palette ───────────────────────────────────────────────────────
        /// <summary>Hex color for the primary accent (e.g. "#1a73e8")</summary>
        public string PrimaryColor { get; set; } = "#1a73e8";

        /// <summary>Hex color for secondary / complementary accent</summary>
        public string SecondaryColor { get; set; } = "#fbbc04";

        /// <summary>Hex for success state indicators</summary>
        public string SuccessColor { get; set; } = "#34a853";

        /// <summary>Hex for warning state indicators</summary>
        public string WarningColor { get; set; } = "#ea8600";

        /// <summary>Hex for danger / error state indicators</summary>
        public string DangerColor { get; set; } = "#ea4335";

        // ── Background & Surface ──────────────────────────────────────────────────
        /// <summary>Main page background color (hex)</summary>
        public string? BackgroundColor { get; set; }

        /// <summary>Card / panel surface color (hex)</summary>
        public string? SurfaceColor { get; set; }

        /// <summary>Sidebar background color; null = inherit from theme</summary>
        public string? SidebarColor { get; set; }

        /// <summary>Top navbar background color; null = inherit from theme</summary>
        public string? NavbarColor { get; set; }

        // ── Typography ────────────────────────────────────────────────────────────
        /// <summary>Font family name, e.g. "Inter", "Roboto", "Nunito"</summary>
        public string FontFamily { get; set; } = "Inter";

        /// <summary>Base body font size: "sm" | "md" | "lg"</summary>
        public string FontSize { get; set; } = "md";

        // ── Layout & Sidebar ──────────────────────────────────────────────────────
        /// <summary>Sidebar layout: "Vertical" | "Horizontal" | "Mini" | "Compact"</summary>
        public string SidebarLayout { get; set; } = "Vertical";

        /// <summary>Start sidebar in collapsed/mini mode by default</summary>
        public bool SidebarCollapsed { get; set; } = false;

        /// <summary>Content area width: "Fluid" | "Boxed"</summary>
        public string ContentWidth { get; set; } = "Fluid";

        // ── Density / Spacing ─────────────────────────────────────────────────────
        /// <summary>UI density: "Comfortable" | "Compact" | "Cozy"</summary>
        public string Density { get; set; } = "Comfortable";

        // ── Visual Effects ────────────────────────────────────────────────────────
        public bool EnableAnimations { get; set; } = true;
        public bool EnableShadows    { get; set; } = true;
        public bool EnableGradients  { get; set; } = false;

        /// <summary>Border radius scale: "None" | "Sm" | "Md" | "Lg" | "Full"</summary>
        public string BorderRadius { get; set; } = "Md";

        // ── Preset Theme ──────────────────────────────────────────────────────────
        /// <summary>Named preset applied (e.g. "Default", "Ocean", "Sunset", "Forest", "Midnight").
        /// Null means custom / manual overrides only.</summary>
        public string? PresetTheme { get; set; } = "Default";

        // ── Timestamps ───────────────────────────────────────────────────────────
        public DateTime LastChangedAt { get; set; } = DateTime.UtcNow;
    }

    public class User : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Staff;
        public int? TenantId { get; set; }
        public Business? Tenant { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public string? OTP { get; set; }
        public DateTime? OTPExpiry { get; set; }
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    public class Permission : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Module { get; set; }
        public string? Description { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    public class RolePermission
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }
    }

    public class Staff : TenantEntity
    {
        public int UserId { get; set; }
        public User? User { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? JoiningDate { get; set; }
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
    }
}
