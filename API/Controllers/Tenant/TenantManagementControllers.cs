using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Services.Interfaces;

namespace BillingSoftware.API.Controllers.Tenant
{
    // =====================================================================
    // WAREHOUSE CONTROLLER
    // =====================================================================
    /// <summary>
    /// Manage warehouses / storage locations within a shop.
    /// All endpoints are tenant-scoped via JWT.
    /// </summary>
    [ApiController]
    [Route("api/warehouses")]
    [Authorize]
    [Produces("application/json")]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _svc;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        public WarehouseController(IWarehouseService svc) => _svc = svc;

        /// <summary>List all warehouses. Optionally filter by branch.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<WarehouseResponse>>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] int? branchId = null)
        {
            var result = await _svc.GetAllAsync(TenantId, branchId);
            return Ok(ApiResponse<List<WarehouseResponse>>.SuccessResponse(result));
        }

        /// <summary>Get a single warehouse by ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<WarehouseResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _svc.GetByIdAsync(id, TenantId);
            if (result == null) return NotFound(ApiResponse<object>.FailResponse($"Warehouse {id} not found"));
            return Ok(ApiResponse<WarehouseResponse>.SuccessResponse(result));
        }

        /// <summary>Create a new warehouse.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WarehouseResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Create([FromBody] CreateWarehouseRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.FailResponse("Invalid request"));
            var result = await _svc.CreateAsync(request, TenantId);
            return Ok(ApiResponse<WarehouseResponse>.SuccessResponse(result, "Warehouse created successfully"));
        }

        /// <summary>Update an existing warehouse.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<WarehouseResponse>), 200)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateWarehouseRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.FailResponse("Invalid request"));
            var result = await _svc.UpdateAsync(id, request, TenantId);
            return Ok(ApiResponse<WarehouseResponse>.SuccessResponse(result, "Warehouse updated successfully"));
        }

        /// <summary>Delete (soft-delete) a warehouse.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteAsync(id, TenantId);
            if (!ok) return NotFound(ApiResponse<object>.FailResponse($"Warehouse {id} not found"));
            return Ok(ApiResponse<object>.SuccessResponse(null, "Warehouse deleted"));
        }

        /// <summary>Mark a warehouse as the default for its branch/tenant.</summary>
        [HttpPost("{id:int}/set-default")]
        [ProducesResponseType(typeof(ApiResponse<WarehouseResponse>), 200)]
        public async Task<IActionResult> SetDefault(int id)
        {
            var result = await _svc.SetDefaultAsync(id, TenantId);
            return Ok(ApiResponse<WarehouseResponse>.SuccessResponse(result, "Default warehouse updated"));
        }
    }

    // =====================================================================
    // COUNTER CONTROLLER
    // =====================================================================
    /// <summary>
    /// Manage POS billing counters within branches.
    /// </summary>
    [ApiController]
    [Route("api/counters")]
    [Authorize]
    [Produces("application/json")]
    public class CounterController : ControllerBase
    {
        private readonly ICounterService _svc;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        public CounterController(ICounterService svc) => _svc = svc;

        /// <summary>List all counters. Optionally filter by branch.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CounterResponse>>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] int? branchId = null)
            => Ok(ApiResponse<List<CounterResponse>>.SuccessResponse(await _svc.GetAllAsync(TenantId, branchId)));

        /// <summary>Get a single counter.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CounterResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _svc.GetByIdAsync(id, TenantId);
            if (result == null) return NotFound(ApiResponse<object>.FailResponse($"Counter {id} not found"));
            return Ok(ApiResponse<CounterResponse>.SuccessResponse(result));
        }

        /// <summary>Create a new counter.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CounterResponse>), 200)]
        public async Task<IActionResult> Create([FromBody] CreateCounterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.FailResponse("Invalid request"));
            var result = await _svc.CreateAsync(request, TenantId);
            return Ok(ApiResponse<CounterResponse>.SuccessResponse(result, "Counter created successfully"));
        }

        /// <summary>Update a counter.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CounterResponse>), 200)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCounterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.FailResponse("Invalid request"));
            return Ok(ApiResponse<CounterResponse>.SuccessResponse(await _svc.UpdateAsync(id, request, TenantId), "Counter updated"));
        }

        /// <summary>Delete a counter.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteAsync(id, TenantId);
            if (!ok) return NotFound(ApiResponse<object>.FailResponse($"Counter {id} not found"));
            return Ok(ApiResponse<object>.SuccessResponse(null, "Counter deleted"));
        }

        /// <summary>Assign a user to a counter.</summary>
        [HttpPost("{id:int}/assign-user")]
        [ProducesResponseType(typeof(ApiResponse<CounterResponse>), 200)]
        public async Task<IActionResult> AssignUser(int id, [FromBody] AssignCounterUserRequest request)
        {
            var result = await _svc.AssignUserAsync(id, request.UserId, TenantId);
            return Ok(ApiResponse<CounterResponse>.SuccessResponse(result, "User assigned to counter"));
        }

        /// <summary>Set counter online/offline status (heartbeat ping).</summary>
        [HttpPost("{id:int}/status")]
        [ProducesResponseType(typeof(ApiResponse<CounterResponse>), 200)]
        public async Task<IActionResult> SetStatus(int id, [FromQuery] bool isOnline)
        {
            var result = await _svc.SetOnlineStatusAsync(id, isOnline, TenantId);
            return Ok(ApiResponse<CounterResponse>.SuccessResponse(result, $"Counter is now {(isOnline ? "online" : "offline")}"));
        }
    }

    // =====================================================================
    // EMPLOYEE ROLE CONTROLLER
    // =====================================================================
    /// <summary>
    /// Manage named employee roles and their granular permission sets.
    /// </summary>
    [ApiController]
    [Route("api/roles")]
    [Authorize]
    [Produces("application/json")]
    public class EmployeeRoleController : ControllerBase
    {
        private readonly IEmployeeRoleService _svc;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        public EmployeeRoleController(IEmployeeRoleService svc) => _svc = svc;

        /// <summary>List all roles for this tenant.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<EmployeeRoleResponse>>), 200)]
        public async Task<IActionResult> GetAll()
            => Ok(ApiResponse<List<EmployeeRoleResponse>>.SuccessResponse(await _svc.GetAllAsync(TenantId)));

        /// <summary>Get a role by ID (includes its permissions).</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeRoleResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _svc.GetByIdAsync(id, TenantId);
            if (result == null) return NotFound(ApiResponse<object>.FailResponse($"Role {id} not found"));
            return Ok(ApiResponse<EmployeeRoleResponse>.SuccessResponse(result));
        }

        /// <summary>Create a new role with optional permissions.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmployeeRoleResponse>), 200)]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRoleRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.FailResponse("Invalid request"));
            return Ok(ApiResponse<EmployeeRoleResponse>.SuccessResponse(await _svc.CreateAsync(request, TenantId), "Role created"));
        }

        /// <summary>Update a role (system roles cannot be modified).</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeRoleResponse>), 200)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeRoleRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.FailResponse("Invalid request"));
            return Ok(ApiResponse<EmployeeRoleResponse>.SuccessResponse(await _svc.UpdateAsync(id, request, TenantId), "Role updated"));
        }

        /// <summary>Delete a role (system roles cannot be deleted).</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteAsync(id, TenantId);
            if (!ok) return NotFound(ApiResponse<object>.FailResponse($"Role {id} not found"));
            return Ok(ApiResponse<object>.SuccessResponse(null, "Role deleted"));
        }

        /// <summary>Replace the full permission set of a role.</summary>
        [HttpPut("{id:int}/permissions")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeRoleResponse>), 200)]
        public async Task<IActionResult> SetPermissions(int id, [FromBody] SetRolePermissionsRequest request)
            => Ok(ApiResponse<EmployeeRoleResponse>.SuccessResponse(await _svc.SetPermissionsAsync(id, request, TenantId), "Permissions updated"));

        /// <summary>Get all available platform permission keys grouped by module.</summary>
        [HttpGet("permissions")]
        [ProducesResponseType(typeof(ApiResponse<List<PermissionGroupResponse>>), 200)]
        public async Task<IActionResult> GetAllPermissions()
            => Ok(ApiResponse<List<PermissionGroupResponse>>.SuccessResponse(await _svc.GetAllPermissionsAsync()));

        /// <summary>List users assigned to a specific role.</summary>
        [HttpGet("{id:int}/users")]
        [ProducesResponseType(typeof(ApiResponse<List<UserRoleResponse>>), 200)]
        public async Task<IActionResult> GetUsers(int id)
            => Ok(ApiResponse<List<UserRoleResponse>>.SuccessResponse(await _svc.GetUsersInRoleAsync(id, TenantId)));
    }

    // =====================================================================
    // USER ROLE CONTROLLER
    // =====================================================================
    /// <summary>
    /// Assign and revoke employee roles for users within the tenant.
    /// </summary>
    [ApiController]
    [Route("api/team")]
    [Authorize]
    [Produces("application/json")]
    public class TeamController : ControllerBase
    {
        private readonly IUserRoleService _svc;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private int UserId   => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        public TeamController(IUserRoleService svc) => _svc = svc;

        /// <summary>List all active team members and their roles.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<UserRoleResponse>>), 200)]
        public async Task<IActionResult> GetTeam()
            => Ok(ApiResponse<List<UserRoleResponse>>.SuccessResponse(await _svc.GetTenantUsersAsync(TenantId)));

        /// <summary>Get roles assigned to a specific user.</summary>
        [HttpGet("users/{userId:int}/roles")]
        [ProducesResponseType(typeof(ApiResponse<List<UserRoleResponse>>), 200)]
        public async Task<IActionResult> GetUserRoles(int userId)
            => Ok(ApiResponse<List<UserRoleResponse>>.SuccessResponse(await _svc.GetUserRolesAsync(userId, TenantId)));

        /// <summary>Get all permission keys for a specific user (flattened across all roles).</summary>
        [HttpGet("users/{userId:int}/permissions")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
        public async Task<IActionResult> GetUserPermissions(int userId)
            => Ok(ApiResponse<List<string>>.SuccessResponse(await _svc.GetUserPermissionsAsync(userId, TenantId)));

        /// <summary>Get the currently authenticated user's permissions.</summary>
        [HttpGet("me/permissions")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
        public async Task<IActionResult> GetMyPermissions()
            => Ok(ApiResponse<List<string>>.SuccessResponse(await _svc.GetUserPermissionsAsync(UserId, TenantId)));

        /// <summary>Assign a role to a user (optionally scoped to a branch/counter).</summary>
        [HttpPost("assign")]
        [ProducesResponseType(typeof(ApiResponse<UserRoleResponse>), 200)]
        public async Task<IActionResult> Assign([FromBody] AssignUserRoleRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.FailResponse("Invalid request"));
            return Ok(ApiResponse<UserRoleResponse>.SuccessResponse(await _svc.AssignRoleAsync(request, TenantId), "Role assigned"));
        }

        /// <summary>Revoke a role from a user.</summary>
        [HttpPost("revoke")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> Revoke([FromBody] RevokeUserRoleRequest request)
        {
            var ok = await _svc.RevokeRoleAsync(request, TenantId);
            if (!ok) return NotFound(ApiResponse<object>.FailResponse("Assignment not found"));
            return Ok(ApiResponse<object>.SuccessResponse(null, "Role revoked successfully"));
        }
    }

    // =====================================================================
    // SHOP INVITE CONTROLLER
    // =====================================================================
    /// <summary>
    /// Send and manage team invitations for the shop.
    /// </summary>
    [ApiController]
    [Route("api/invites")]
    [Authorize]
    [Produces("application/json")]
    public class ShopInviteController : ControllerBase
    {
        private readonly IShopInviteService _svc;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private int UserId   => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        public ShopInviteController(IShopInviteService svc) => _svc = svc;

        /// <summary>List all invites sent for this shop.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ShopInviteResponse>>), 200)]
        public async Task<IActionResult> GetAll()
            => Ok(ApiResponse<List<ShopInviteResponse>>.SuccessResponse(await _svc.GetAllAsync(TenantId)));

        /// <summary>Send a new invite email to a team member.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ShopInviteResponse>), 200)]
        public async Task<IActionResult> Send([FromBody] SendShopInviteRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.FailResponse("Invalid request"));
            return Ok(ApiResponse<ShopInviteResponse>.SuccessResponse(await _svc.SendInviteAsync(request, TenantId, UserId), "Invitation sent"));
        }

        /// <summary>Accept an invite using the token from the invitation email. (No auth required)</summary>
        [HttpPost("accept")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ShopInviteResponse>), 200)]
        public async Task<IActionResult> Accept([FromBody] AcceptShopInviteRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.FailResponse("Invalid request"));
            var result = await _svc.AcceptInviteAsync(request);
            return Ok(ApiResponse<ShopInviteResponse>.SuccessResponse(result, "Invitation accepted. Please log in."));
        }

        /// <summary>Cancel a pending invite.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> Cancel(int id)
        {
            var ok = await _svc.CancelInviteAsync(id, TenantId);
            if (!ok) return NotFound(ApiResponse<object>.FailResponse($"Invite {id} not found"));
            return Ok(ApiResponse<object>.SuccessResponse(null, "Invite cancelled"));
        }

        /// <summary>Resend an existing invite (extends expiry by 7 days).</summary>
        [HttpPost("{id:int}/resend")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> Resend(int id)
        {
            var ok = await _svc.ResendInviteAsync(id, TenantId);
            if (!ok) return NotFound(ApiResponse<object>.FailResponse($"Invite {id} not found"));
            return Ok(ApiResponse<object>.SuccessResponse(null, "Invite resent successfully"));
        }
    }

    // =====================================================================
    // SUBSCRIPTION USAGE CONTROLLER
    // =====================================================================
    /// <summary>
    /// Monitor subscription usage limits and alerts in real time.
    /// </summary>
    [ApiController]
    [Route("api/usage")]
    [Authorize]
    [Produces("application/json")]
    public class SubscriptionUsageController : ControllerBase
    {
        private readonly ISubscriptionUsageService _svc;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private int UserId   => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        public SubscriptionUsageController(ISubscriptionUsageService svc) => _svc = svc;

        /// <summary>Get current month's usage vs plan limits, with percentage bars.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<SubscriptionUsageResponse>), 200)]
        public async Task<IActionResult> GetCurrent()
            => Ok(ApiResponse<SubscriptionUsageResponse>.SuccessResponse(await _svc.GetCurrentUsageAsync(TenantId)));

        /// <summary>Get usage history for the last N months.</summary>
        [HttpGet("history")]
        [ProducesResponseType(typeof(ApiResponse<List<SubscriptionUsageResponse>>), 200)]
        public async Task<IActionResult> GetHistory([FromQuery] int months = 6)
            => Ok(ApiResponse<List<SubscriptionUsageResponse>>.SuccessResponse(await _svc.GetUsageHistoryAsync(TenantId, months)));

        /// <summary>Force recalculate the usage counts from live data (admin/debug utility).</summary>
        [HttpPost("recalculate")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> Recalculate()
        {
            await _svc.RecalculateAsync(TenantId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Usage recalculated"));
        }

        /// <summary>Get active usage alerts (80%, 90%, 100% threshold breaches).</summary>
        [HttpGet("alerts")]
        [ProducesResponseType(typeof(ApiResponse<List<UsageAlertResponse>>), 200)]
        public async Task<IActionResult> GetAlerts()
            => Ok(ApiResponse<List<UsageAlertResponse>>.SuccessResponse(await _svc.GetActiveAlertsAsync(TenantId)));

        /// <summary>Acknowledge (dismiss) a usage alert.</summary>
        [HttpPost("alerts/{alertId:int}/acknowledge")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> Acknowledge(int alertId)
        {
            await _svc.AcknowledgeAlertAsync(alertId, TenantId, UserId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Alert acknowledged"));
        }

        /// <summary>Check whether the tenant is within a specific limit (Invoices/Products/Users/Branches).</summary>
        [HttpGet("check/{limitType}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> CheckLimit(string limitType)
        {
            var ok = await _svc.IsWithinLimitAsync(TenantId, limitType);
            return Ok(ApiResponse<bool>.SuccessResponse(ok, ok ? "Within limit" : "Limit reached"));
        }
    }

    // =====================================================================
    // AI INSIGHTS CONTROLLER
    // =====================================================================
    /// <summary>
    /// AI-powered sales summaries, demand forecasts, reorder suggestions and anomaly alerts.
    /// </summary>
    [ApiController]
    [Route("api/ai")]
    [Authorize]
    [Produces("application/json")]
    public class AIInsightController : ControllerBase
    {
        private readonly IAIInsightService _svc;
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        private int UserId   => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        public AIInsightController(IAIInsightService svc) => _svc = svc;

        /// <summary>
        /// List AI insights. Filter by type or unread status.
        /// Types: SalesSummary | DemandForecast | ReorderSuggestion | AnomalyAlert | CustomerInsight
        /// </summary>
        [HttpGet("insights")]
        [ProducesResponseType(typeof(ApiResponse<List<AIInsightResponse>>), 200)]
        public async Task<IActionResult> GetInsights(
            [FromQuery] string? type       = null,
            [FromQuery] bool    unreadOnly = false,
            [FromQuery] int     page       = 1,
            [FromQuery] int     pageSize   = 20)
            => Ok(ApiResponse<List<AIInsightResponse>>.SuccessResponse(
                await _svc.GetInsightsAsync(TenantId, type, unreadOnly, page, pageSize)));

        /// <summary>Get a single insight by ID.</summary>
        [HttpGet("insights/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AIInsightResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _svc.GetByIdAsync(id, TenantId);
            if (result == null) return NotFound(ApiResponse<object>.FailResponse($"Insight {id} not found"));
            return Ok(ApiResponse<AIInsightResponse>.SuccessResponse(result));
        }

        /// <summary>Unread insight count (badge counter for nav bar).</summary>
        [HttpGet("insights/unread-count")]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        public async Task<IActionResult> UnreadCount()
            => Ok(ApiResponse<int>.SuccessResponse(await _svc.GetUnreadCountAsync(TenantId)));

        /// <summary>
        /// Generate a new AI insight on demand.
        /// Use type = SalesSummary | DemandForecast | ReorderSuggestion | AnomalyAlert | CustomerInsight
        /// </summary>
        [HttpPost("insights/generate")]
        [ProducesResponseType(typeof(ApiResponse<AIInsightResponse>), 200)]
        public async Task<IActionResult> Generate([FromBody] GenerateInsightRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.FailResponse("Invalid request"));
            return Ok(ApiResponse<AIInsightResponse>.SuccessResponse(
                await _svc.GenerateInsightAsync(request, TenantId, UserId), "Insight generated"));
        }

        /// <summary>Mark an insight as read / acted on.</summary>
        [HttpPatch("insights/{id:int}/read")]
        [ProducesResponseType(typeof(ApiResponse<AIInsightResponse>), 200)]
        public async Task<IActionResult> MarkRead(int id, [FromBody] MarkInsightReadRequest request)
            => Ok(ApiResponse<AIInsightResponse>.SuccessResponse(await _svc.MarkReadAsync(id, TenantId, request)));

        /// <summary>Delete an insight.</summary>
        [HttpDelete("insights/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteInsightAsync(id, TenantId);
            if (!ok) return NotFound(ApiResponse<object>.FailResponse($"Insight {id} not found"));
            return Ok(ApiResponse<object>.SuccessResponse(null, "Insight deleted"));
        }

        // ── Quick shortcut endpoints ──────────────────────────────────────────────

        /// <summary>Quick endpoint: get today's sales summary.</summary>
        [HttpGet("sales-summary")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> SalesSummary(
            [FromQuery] DateTime? from     = null,
            [FromQuery] DateTime? to       = null,
            [FromQuery] int?      branchId = null)
        {
            var result = await _svc.GetSalesSummaryAsync(
                TenantId,
                from ?? DateTime.UtcNow.Date,
                to   ?? DateTime.UtcNow,
                branchId);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        /// <summary>Quick endpoint: get products that need reordering.</summary>
        [HttpGet("reorder-suggestions")]
        [ProducesResponseType(typeof(ApiResponse<List<object>>), 200)]
        public async Task<IActionResult> ReorderSuggestions([FromQuery] int? branchId = null)
            => Ok(ApiResponse<List<object>>.SuccessResponse(await _svc.GetReorderSuggestionsAsync(TenantId, branchId)));

        // ── Demand forecasts ──────────────────────────────────────────────────────

        /// <summary>List demand forecasts for one or all products.</summary>
        [HttpGet("forecasts")]
        [ProducesResponseType(typeof(ApiResponse<List<AIForecastResponse>>), 200)]
        public async Task<IActionResult> GetForecasts(
            [FromQuery] int?   productId   = null,
            [FromQuery] string granularity = "Day",
            [FromQuery] int    days        = 30)
            => Ok(ApiResponse<List<AIForecastResponse>>.SuccessResponse(
                await _svc.GetForecastsAsync(TenantId, productId, granularity, days)));

        /// <summary>Generate / refresh a demand forecast for a specific product.</summary>
        [HttpPost("forecasts/generate/{productId:int}")]
        [ProducesResponseType(typeof(ApiResponse<AIForecastResponse>), 200)]
        public async Task<IActionResult> GenerateForecast(int productId, [FromQuery] string granularity = "Day")
            => Ok(ApiResponse<AIForecastResponse>.SuccessResponse(
                await _svc.GenerateForecastAsync(TenantId, productId, granularity, UserId), "Forecast generated"));
    }
}
