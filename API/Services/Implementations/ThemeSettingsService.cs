using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingSoftware.API.Services.Implementations
{
    // ==================== THEME SETTINGS SERVICE ====================
    public class ThemeSettingsService : IThemeSettingsService
    {
        private readonly ApplicationDbContext _context;

        public ThemeSettingsService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Preset catalogue ─────────────────────────────────────────────────────

        /// <summary>
        /// All built-in theme presets. Add new entries here to extend the catalogue.
        /// </summary>
        private static readonly List<ThemePresetInfo> _presets = new()
        {
            new ThemePresetInfo
            {
                Name           = "Default",
                DisplayName    = "Default Blue",
                Description    = "Clean corporate blue — the factory default.",
                PrimaryColor   = "#1a73e8",
                SecondaryColor = "#fbbc04",
                ColorMode      = "Light",
                PreviewColors  = new() { "#1a73e8", "#fbbc04", "#34a853", "#ea4335" }
            },
            new ThemePresetInfo
            {
                Name           = "Ocean",
                DisplayName    = "Ocean Breeze",
                Description    = "Deep teal tones inspired by the open sea.",
                PrimaryColor   = "#0097a7",
                SecondaryColor = "#00bcd4",
                ColorMode      = "Light",
                PreviewColors  = new() { "#0097a7", "#00bcd4", "#26a69a", "#80deea" }
            },
            new ThemePresetInfo
            {
                Name           = "Sunset",
                DisplayName    = "Sunset Glow",
                Description    = "Warm oranges and deep purples — bold and energetic.",
                PrimaryColor   = "#e64a19",
                SecondaryColor = "#ff7043",
                ColorMode      = "Light",
                PreviewColors  = new() { "#e64a19", "#ff7043", "#ab47bc", "#ffa726" }
            },
            new ThemePresetInfo
            {
                Name           = "Forest",
                DisplayName    = "Forest Green",
                Description    = "Natural greens with earthy neutrals — calm and professional.",
                PrimaryColor   = "#2e7d32",
                SecondaryColor = "#66bb6a",
                ColorMode      = "Light",
                PreviewColors  = new() { "#2e7d32", "#66bb6a", "#a5d6a7", "#795548" }
            },
            new ThemePresetInfo
            {
                Name           = "Midnight",
                DisplayName    = "Midnight Dark",
                Description    = "Full dark mode with indigo accents — easy on the eyes.",
                PrimaryColor   = "#7c4dff",
                SecondaryColor = "#b388ff",
                ColorMode      = "Dark",
                PreviewColors  = new() { "#7c4dff", "#b388ff", "#121212", "#1e1e1e" }
            }
        };

        /// <summary>
        /// Palette overrides applied when a preset is selected.
        /// Tuple: (primary, secondary, success, warning, danger, colorMode)
        /// </summary>
        private static readonly Dictionary<string, (string Primary, string Secondary, string Success, string Warning, string Danger, string ColorMode)>
            _presetValues = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Default"]  = ("#1a73e8", "#fbbc04", "#34a853", "#ea8600", "#ea4335", "Light"),
                ["Ocean"]    = ("#0097a7", "#00bcd4", "#26a69a", "#f57c00", "#ef5350", "Light"),
                ["Sunset"]   = ("#e64a19", "#ff7043", "#43a047", "#ffa000", "#ab47bc", "Light"),
                ["Forest"]   = ("#2e7d32", "#66bb6a", "#00897b", "#f9a825", "#e53935", "Light"),
                ["Midnight"] = ("#7c4dff", "#b388ff", "#00e676", "#ffab40", "#ff5252", "Dark"),
            };

        // ── Public API ───────────────────────────────────────────────────────────

        public async Task<List<ThemePresetInfo>> GetPresetsAsync()
            => await Task.FromResult(_presets);

        public async Task<ThemeSettingsResponse> GetThemeSettingsAsync(int userId, int tenantId)
        {
            var settings = await FindAsync(userId, tenantId);
            if (settings == null)
            {
                settings = CreateDefaults(userId, tenantId);
                _context.UserThemeSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return MapToResponse(settings);
        }

        public async Task<ThemeSettingsResponse> UpdateThemeSettingsAsync(
            int userId, int tenantId, UpdateThemeSettingsRequest request)
        {
            var settings = await FindAsync(userId, tenantId) ?? CreateDefaults(userId, tenantId);

            ApplyFullUpdate(settings, request);
            settings.PresetTheme   = request.PresetTheme;
            settings.LastChangedAt = DateTime.UtcNow;

            Upsert(settings);
            await _context.SaveChangesAsync();
            return MapToResponse(settings);
        }

        public async Task<ThemeSettingsResponse> PatchThemeSettingsAsync(
            int userId, int tenantId, PatchThemeSettingsRequest request)
        {
            var settings = await FindAsync(userId, tenantId) ?? CreateDefaults(userId, tenantId);

            if (request.ColorMode        != null) settings.ColorMode       = request.ColorMode;
            if (request.PrimaryColor     != null) settings.PrimaryColor    = request.PrimaryColor;
            if (request.SecondaryColor   != null) settings.SecondaryColor  = request.SecondaryColor;
            if (request.SuccessColor     != null) settings.SuccessColor    = request.SuccessColor;
            if (request.WarningColor     != null) settings.WarningColor    = request.WarningColor;
            if (request.DangerColor      != null) settings.DangerColor     = request.DangerColor;
            if (request.BackgroundColor  != null) settings.BackgroundColor = request.BackgroundColor;
            if (request.SurfaceColor     != null) settings.SurfaceColor    = request.SurfaceColor;
            if (request.SidebarColor     != null) settings.SidebarColor    = request.SidebarColor;
            if (request.NavbarColor      != null) settings.NavbarColor     = request.NavbarColor;
            if (request.FontFamily       != null) settings.FontFamily      = request.FontFamily;
            if (request.FontSize         != null) settings.FontSize        = request.FontSize;
            if (request.SidebarLayout    != null) settings.SidebarLayout   = request.SidebarLayout;
            if (request.SidebarCollapsed.HasValue)  settings.SidebarCollapsed = request.SidebarCollapsed.Value;
            if (request.ContentWidth     != null) settings.ContentWidth    = request.ContentWidth;
            if (request.Density          != null) settings.Density         = request.Density;
            if (request.EnableAnimations.HasValue)  settings.EnableAnimations = request.EnableAnimations.Value;
            if (request.EnableShadows   .HasValue)  settings.EnableShadows    = request.EnableShadows.Value;
            if (request.EnableGradients .HasValue)  settings.EnableGradients  = request.EnableGradients.Value;
            if (request.BorderRadius     != null) settings.BorderRadius    = request.BorderRadius;
            if (request.PresetTheme      != null) settings.PresetTheme     = request.PresetTheme;

            // Manual color changes break the preset — mark as custom
            if (HasAnyColorOverride(request))
                settings.PresetTheme = null;

            settings.LastChangedAt = DateTime.UtcNow;

            Upsert(settings);
            await _context.SaveChangesAsync();
            return MapToResponse(settings);
        }

        public async Task<ThemeSettingsResponse> ResetToDefaultsAsync(int userId, int tenantId)
        {
            var settings = await FindAsync(userId, tenantId);
            if (settings == null)
            {
                settings = CreateDefaults(userId, tenantId);
                _context.UserThemeSettings.Add(settings);
            }
            else
            {
                ResetFields(settings, userId, tenantId);
                _context.UserThemeSettings.Update(settings);
            }
            await _context.SaveChangesAsync();
            return MapToResponse(settings);
        }

        public async Task<ThemeSettingsResponse> ApplyPresetAsync(int userId, int tenantId, string presetName)
        {
            if (!_presetValues.TryGetValue(presetName, out var preset))
                throw new KeyNotFoundException(
                    $"Theme preset '{presetName}' not found. Valid options: {string.Join(", ", _presetValues.Keys)}");

            var settings = await FindAsync(userId, tenantId) ?? CreateDefaults(userId, tenantId);

            settings.PrimaryColor   = preset.Primary;
            settings.SecondaryColor = preset.Secondary;
            settings.SuccessColor   = preset.Success;
            settings.WarningColor   = preset.Warning;
            settings.DangerColor    = preset.Danger;
            settings.ColorMode      = preset.ColorMode;
            settings.PresetTheme    = presetName;
            settings.LastChangedAt  = DateTime.UtcNow;

            Upsert(settings);
            await _context.SaveChangesAsync();
            return MapToResponse(settings);
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private async Task<UserThemeSettings?> FindAsync(int userId, int tenantId)
            => await _context.UserThemeSettings
                .FirstOrDefaultAsync(t => t.UserId == userId && t.TenantId == tenantId && !t.IsDeleted);

        private void Upsert(UserThemeSettings settings)
        {
            if (settings.Id == 0)
                _context.UserThemeSettings.Add(settings);
            else
                _context.UserThemeSettings.Update(settings);
        }

        private static UserThemeSettings CreateDefaults(int userId, int tenantId) => new()
        {
            UserId           = userId,
            TenantId         = tenantId,
            ColorMode        = "Light",
            PrimaryColor     = "#1a73e8",
            SecondaryColor   = "#fbbc04",
            SuccessColor     = "#34a853",
            WarningColor     = "#ea8600",
            DangerColor      = "#ea4335",
            FontFamily       = "Inter",
            FontSize         = "md",
            SidebarLayout    = "Vertical",
            SidebarCollapsed = false,
            ContentWidth     = "Fluid",
            Density          = "Comfortable",
            EnableAnimations = true,
            EnableShadows    = true,
            EnableGradients  = false,
            BorderRadius     = "Md",
            PresetTheme      = "Default",
            LastChangedAt    = DateTime.UtcNow
        };

        private static void ResetFields(UserThemeSettings s, int userId, int tenantId)
        {
            var d = CreateDefaults(userId, tenantId);
            s.ColorMode        = d.ColorMode;
            s.PrimaryColor     = d.PrimaryColor;
            s.SecondaryColor   = d.SecondaryColor;
            s.SuccessColor     = d.SuccessColor;
            s.WarningColor     = d.WarningColor;
            s.DangerColor      = d.DangerColor;
            s.BackgroundColor  = null;
            s.SurfaceColor     = null;
            s.SidebarColor     = null;
            s.NavbarColor      = null;
            s.FontFamily       = d.FontFamily;
            s.FontSize         = d.FontSize;
            s.SidebarLayout    = d.SidebarLayout;
            s.SidebarCollapsed = d.SidebarCollapsed;
            s.ContentWidth     = d.ContentWidth;
            s.Density          = d.Density;
            s.EnableAnimations = d.EnableAnimations;
            s.EnableShadows    = d.EnableShadows;
            s.EnableGradients  = d.EnableGradients;
            s.BorderRadius     = d.BorderRadius;
            s.PresetTheme      = d.PresetTheme;
            s.LastChangedAt    = DateTime.UtcNow;
        }

        private static void ApplyFullUpdate(UserThemeSettings s, UpdateThemeSettingsRequest r)
        {
            s.ColorMode        = r.ColorMode;
            s.PrimaryColor     = r.PrimaryColor;
            s.SecondaryColor   = r.SecondaryColor;
            s.SuccessColor     = r.SuccessColor;
            s.WarningColor     = r.WarningColor;
            s.DangerColor      = r.DangerColor;
            s.BackgroundColor  = r.BackgroundColor;
            s.SurfaceColor     = r.SurfaceColor;
            s.SidebarColor     = r.SidebarColor;
            s.NavbarColor      = r.NavbarColor;
            s.FontFamily       = r.FontFamily;
            s.FontSize         = r.FontSize;
            s.SidebarLayout    = r.SidebarLayout;
            s.SidebarCollapsed = r.SidebarCollapsed;
            s.ContentWidth     = r.ContentWidth;
            s.Density          = r.Density;
            s.EnableAnimations = r.EnableAnimations;
            s.EnableShadows    = r.EnableShadows;
            s.EnableGradients  = r.EnableGradients;
            s.BorderRadius     = r.BorderRadius;
        }

        private static bool HasAnyColorOverride(PatchThemeSettingsRequest r)
            => r.PrimaryColor    != null || r.SecondaryColor != null ||
               r.SuccessColor    != null || r.WarningColor   != null ||
               r.DangerColor     != null || r.BackgroundColor != null ||
               r.SurfaceColor    != null || r.SidebarColor   != null ||
               r.NavbarColor     != null || r.ColorMode      != null;

        // ── Mapping ──────────────────────────────────────────────────────────────

        private static ThemeSettingsResponse MapToResponse(UserThemeSettings s) => new()
        {
            Id               = s.Id,
            UserId           = s.UserId,
            TenantId         = s.TenantId,
            ColorMode        = s.ColorMode,
            PrimaryColor     = s.PrimaryColor,
            SecondaryColor   = s.SecondaryColor,
            SuccessColor     = s.SuccessColor,
            WarningColor     = s.WarningColor,
            DangerColor      = s.DangerColor,
            BackgroundColor  = s.BackgroundColor,
            SurfaceColor     = s.SurfaceColor,
            SidebarColor     = s.SidebarColor,
            NavbarColor      = s.NavbarColor,
            FontFamily       = s.FontFamily,
            FontSize         = s.FontSize,
            SidebarLayout    = s.SidebarLayout,
            SidebarCollapsed = s.SidebarCollapsed,
            ContentWidth     = s.ContentWidth,
            Density          = s.Density,
            EnableAnimations = s.EnableAnimations,
            EnableShadows    = s.EnableShadows,
            EnableGradients  = s.EnableGradients,
            BorderRadius     = s.BorderRadius,
            PresetTheme      = s.PresetTheme,
            LastChangedAt    = s.LastChangedAt,
            UpdatedAt        = s.UpdatedAt,
            CssVariables     = BuildCssVariables(s)
        };

        /// <summary>
        /// Generates a CSS custom-property map ready for front-end injection.
        /// Front-end usage:
        ///   Object.entries(theme.cssVariables).forEach(([k, v]) =>
        ///     document.documentElement.style.setProperty(k, v));
        /// </summary>
        private static Dictionary<string, string> BuildCssVariables(UserThemeSettings s)
        {
            var vars = new Dictionary<string, string>
            {
                ["--color-primary"]      = s.PrimaryColor,
                ["--color-secondary"]    = s.SecondaryColor,
                ["--color-success"]      = s.SuccessColor,
                ["--color-warning"]      = s.WarningColor,
                ["--color-danger"]       = s.DangerColor,
                ["--font-family-base"]   = $"'{s.FontFamily}', sans-serif",
                ["--font-size-base"]     = s.FontSize switch
                {
                    "sm" => "13px",
                    "lg" => "16px",
                    _    => "14px"   // md
                },
                ["--border-radius-base"] = s.BorderRadius switch
                {
                    "None" => "0",
                    "Sm"   => "4px",
                    "Lg"   => "12px",
                    "Full" => "9999px",
                    _      => "8px"   // Md
                },
                ["--color-mode"]         = s.ColorMode.ToLower(),
                ["--sidebar-width"]      = s.SidebarCollapsed ? "64px" : "260px",
                ["--content-max-width"]  = s.ContentWidth == "Boxed" ? "1200px" : "100%",
                ["--animations-enabled"] = s.EnableAnimations ? "1" : "0",
                ["--shadows-enabled"]    = s.EnableShadows    ? "1" : "0",
                ["--gradients-enabled"]  = s.EnableGradients  ? "1" : "0",
            };

            if (!string.IsNullOrWhiteSpace(s.BackgroundColor))
                vars["--color-background"]  = s.BackgroundColor;
            if (!string.IsNullOrWhiteSpace(s.SurfaceColor))
                vars["--color-surface"]     = s.SurfaceColor;
            if (!string.IsNullOrWhiteSpace(s.SidebarColor))
                vars["--color-sidebar-bg"]  = s.SidebarColor;
            if (!string.IsNullOrWhiteSpace(s.NavbarColor))
                vars["--color-navbar-bg"]   = s.NavbarColor;

            return vars;
        }
    }
}
