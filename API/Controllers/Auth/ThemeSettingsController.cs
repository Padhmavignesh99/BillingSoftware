using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Services.Interfaces;
using System.Security.Claims;

namespace MasterBilling.API.Controllers.Auth
{
    /// <summary>
    /// Manages per-user UI theme settings.
    ///
    /// Route convention:
    ///   GET    /api/themesettings           → current user's theme (auto-creates defaults)
    ///   PUT    /api/themesettings           → full replace for current user
    ///   PATCH  /api/themesettings           → partial update for current user
    ///   DELETE /api/themesettings           → reset current user to defaults
    ///   GET    /api/themesettings/presets   → list all built-in presets
    ///   POST   /api/themesettings/preset    → apply a named preset to current user
    ///   GET    /api/themesettings/{userId}  → get another user's theme (Manager/Owner/SuperAdmin)
    ///   PUT    /api/themesettings/{userId}  → replace another user's theme (Manager/Owner/SuperAdmin)
    ///   DELETE /api/themesettings/{userId}/reset → reset another user's theme (Owner/SuperAdmin)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ThemeSettingsController : ControllerBase
    {
        private readonly IThemeSettingsService _service;

        public ThemeSettingsController(IThemeSettingsService service)
        {
            _service = service;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>User ID resolved from the JWT — 0 if not found (shouldn't happen for [Authorize]).</summary>
        private int CurrentUserId
            => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("UserId")?.Value, out var id) ? id : 0;

        /// <summary>Tenant ID for multi-tenant scoping; 0 = SuperAdmin (no tenant).</summary>
        private int TenantId
            => int.TryParse(User.FindFirst("TenantId")?.Value, out var id) ? id : 0;

        // ── Current-user endpoints (self-service) ────────────────────────────────

        /// <summary>
        /// Returns the authenticated user's theme settings.
        /// Automatically creates and persists default settings on first call.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ThemeSettingsResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ThemeSettingsResponse>>> GetMyTheme()
        {
            var result = await _service.GetThemeSettingsAsync(CurrentUserId, TenantId);
            return Ok(ApiResponse<ThemeSettingsResponse>.SuccessResponse(result));
        }

        /// <summary>
        /// Fully replaces (PUT) the authenticated user's theme settings.
        /// All fields must be supplied; null optional fields remove the override.
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<ThemeSettingsResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ThemeSettingsResponse>>> UpdateMyTheme(
            [FromBody] UpdateThemeSettingsRequest request)
        {
            var result = await _service.UpdateThemeSettingsAsync(CurrentUserId, TenantId, request);
            return Ok(ApiResponse<ThemeSettingsResponse>.SuccessResponse(result, "Theme settings updated"));
        }

        /// <summary>
        /// Partially updates (PATCH) the authenticated user's theme settings.
        /// Only non-null fields in the body are applied; everything else remains unchanged.
        /// </summary>
        [HttpPatch]
        [ProducesResponseType(typeof(ApiResponse<ThemeSettingsResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ThemeSettingsResponse>>> PatchMyTheme(
            [FromBody] PatchThemeSettingsRequest request)
        {
            var result = await _service.PatchThemeSettingsAsync(CurrentUserId, TenantId, request);
            return Ok(ApiResponse<ThemeSettingsResponse>.SuccessResponse(result, "Theme settings patched"));
        }

        /// <summary>
        /// Resets the authenticated user's theme settings back to system defaults.
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponse<ThemeSettingsResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ThemeSettingsResponse>>> ResetMyTheme()
        {
            var result = await _service.ResetToDefaultsAsync(CurrentUserId, TenantId);
            return Ok(ApiResponse<ThemeSettingsResponse>.SuccessResponse(result, "Theme reset to defaults"));
        }

        // ── Preset endpoints ─────────────────────────────────────────────────────

        /// <summary>
        /// Returns the full catalogue of available built-in theme presets.
        /// Includes name, display name, description, primary/secondary colors, and a preview palette.
        /// </summary>
        [HttpGet("presets")]
        [ProducesResponseType(typeof(ApiResponse<List<ThemePresetInfo>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ThemePresetInfo>>>> GetPresets()
        {
            var presets = await _service.GetPresetsAsync();
            return Ok(ApiResponse<List<ThemePresetInfo>>.SuccessResponse(presets));
        }

        /// <summary>
        /// Applies a named preset to the authenticated user's theme settings.
        /// Preset colors overwrite current colors; layout / typography settings are preserved.
        /// </summary>
        /// <remarks>
        /// Available presets: Default | Ocean | Sunset | Forest | Midnight
        /// </remarks>
        [HttpPost("preset")]
        [ProducesResponseType(typeof(ApiResponse<ThemeSettingsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ThemeSettingsResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ThemeSettingsResponse>>> ApplyPreset(
            [FromBody] ApplyThemePresetRequest request)
        {
            try
            {
                var result = await _service.ApplyPresetAsync(CurrentUserId, TenantId, request.PresetName);
                return Ok(ApiResponse<ThemeSettingsResponse>.SuccessResponse(
                    result, $"Preset '{request.PresetName}' applied"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<ThemeSettingsResponse>.FailResponse(ex.Message));
            }
        }

        // ── Admin / manager endpoints (target a specific user) ───────────────────

        /// <summary>
        /// Returns the theme settings for a specific user.
        /// Requires Manager, Owner, or SuperAdmin role.
        /// The tenantId query parameter is required for SuperAdmin to scope the lookup correctly.
        /// </summary>
        [HttpGet("{userId:int}")]
        [Authorize(Roles = "Manager,Owner,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<ThemeSettingsResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ThemeSettingsResponse>>> GetUserTheme(
            int userId,
            [FromQuery] int? tenantId = null)
        {
            var tid = tenantId ?? TenantId;
            var result = await _service.GetThemeSettingsAsync(userId, tid);
            return Ok(ApiResponse<ThemeSettingsResponse>.SuccessResponse(result));
        }

        /// <summary>
        /// Fully replaces a specific user's theme settings.
        /// Requires Manager, Owner, or SuperAdmin role.
        /// </summary>
        [HttpPut("{userId:int}")]
        [Authorize(Roles = "Manager,Owner,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<ThemeSettingsResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ThemeSettingsResponse>>> UpdateUserTheme(
            int userId,
            [FromBody] UpdateThemeSettingsRequest request,
            [FromQuery] int? tenantId = null)
        {
            var tid = tenantId ?? TenantId;
            var result = await _service.UpdateThemeSettingsAsync(userId, tid, request);
            return Ok(ApiResponse<ThemeSettingsResponse>.SuccessResponse(result, "User theme updated"));
        }

        /// <summary>
        /// Partially updates a specific user's theme settings (PATCH semantics).
        /// Requires Manager, Owner, or SuperAdmin role.
        /// </summary>
        [HttpPatch("{userId:int}")]
        [Authorize(Roles = "Manager,Owner,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<ThemeSettingsResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ThemeSettingsResponse>>> PatchUserTheme(
            int userId,
            [FromBody] PatchThemeSettingsRequest request,
            [FromQuery] int? tenantId = null)
        {
            var tid = tenantId ?? TenantId;
            var result = await _service.PatchThemeSettingsAsync(userId, tid, request);
            return Ok(ApiResponse<ThemeSettingsResponse>.SuccessResponse(result, "User theme patched"));
        }

        /// <summary>
        /// Resets a specific user's theme to system defaults.
        /// Requires Owner or SuperAdmin role.
        /// </summary>
        [HttpDelete("{userId:int}/reset")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<ThemeSettingsResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ThemeSettingsResponse>>> ResetUserTheme(
            int userId,
            [FromQuery] int? tenantId = null)
        {
            var tid = tenantId ?? TenantId;
            var result = await _service.ResetToDefaultsAsync(userId, tid);
            return Ok(ApiResponse<ThemeSettingsResponse>.SuccessResponse(result, "User theme reset to defaults"));
        }

        /// <summary>
        /// Applies a named preset to a specific user's theme.
        /// Requires Manager, Owner, or SuperAdmin role.
        /// </summary>
        [HttpPost("{userId:int}/preset")]
        [Authorize(Roles = "Manager,Owner,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<ThemeSettingsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ThemeSettingsResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ThemeSettingsResponse>>> ApplyPresetForUser(
            int userId,
            [FromBody] ApplyThemePresetRequest request,
            [FromQuery] int? tenantId = null)
        {
            try
            {
                var tid = tenantId ?? TenantId;
                var result = await _service.ApplyPresetAsync(userId, tid, request.PresetName);
                return Ok(ApiResponse<ThemeSettingsResponse>.SuccessResponse(
                    result, $"Preset '{request.PresetName}' applied to user {userId}"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<ThemeSettingsResponse>.FailResponse(ex.Message));
            }
        }
    }
}
