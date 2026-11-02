using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Models.Enums;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingSoftware.API.Controllers.Auth
{
    /// <summary>
    /// Super-Admin panel: full control over shops, users, subscription plans,
    /// subscriptions, trials, payments, and audit logs.
    ///
    /// Default Admin credentials (seed data):
    ///   SuperAdmin: superadmin@billingsoftware.com / SuperAdmin@123
    ///   Admin:      admin@billingsoftware.com      / Admin@123
    ///
    /// Login via POST /api/auth/login to get a JWT, then authorize with Bearer &lt;token&gt;.
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _admin;
        private readonly ApplicationDbContext _db;
        public AdminController(IAdminService admin, ApplicationDbContext db)
        {
            _admin = admin;
            _db = db;
        }

        // ── DASHBOARD ───────────────────────────────────────────────────────────────

        /// <summary>Platform-wide dashboard statistics</summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<AdminDashboardResponse>>> GetDashboard()
            => Ok(ApiResponse<AdminDashboardResponse>.SuccessResponse(await _admin.GetDashboardAsync(), "Dashboard loaded"));

        /// <summary>Subscription revenue report</summary>
        [HttpGet("revenue")]
        public async Task<ActionResult<ApiResponse<RevenueReportResponse>>> GetRevenue()
            => Ok(ApiResponse<RevenueReportResponse>.SuccessResponse(await _admin.GetRevenueReportAsync(), "Revenue report"));

        // ── SHOPS ───────────────────────────────────────────────────────────────────

        /// <summary>List all registered shops / businesses</summary>
        [HttpGet("shops")]
        public async Task<ActionResult<ApiResponse<List<AdminShopResponse>>>> GetShops()
            => Ok(ApiResponse<List<AdminShopResponse>>.SuccessResponse(await _admin.GetAllShopsAsync(), "Shops loaded"));

        /// <summary>Get a single shop by ID</summary>
        [HttpGet("shops/{id}")]
        public async Task<ActionResult<ApiResponse<AdminShopResponse>>> GetShop(int id)
        {
            var shop = await _admin.GetShopByIdAsync(id);
            if (shop == null) return NotFound(ApiResponse<object>.FailResponse("Shop not found"));
            return Ok(ApiResponse<AdminShopResponse>.SuccessResponse(shop));
        }

        /// <summary>Update shop details</summary>
        [HttpPut("shops/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateShop(int id, [FromBody] AdminUpdateShopRequest request)
            => Ok(ApiResponse<bool>.SuccessResponse(await _admin.UpdateShopAsync(id, request), "Shop updated"));

        /// <summary>Suspend a shop (blocks login for all its users)</summary>
        [HttpPost("shops/{id}/suspend")]
        public async Task<ActionResult<ApiResponse<bool>>> SuspendShop(int id, [FromQuery] string? reason)
            => Ok(ApiResponse<bool>.SuccessResponse(await _admin.SuspendShopAsync(id, reason), "Shop suspended"));

        /// <summary>Reactivate a suspended shop</summary>
        [HttpPost("shops/{id}/activate")]
        public async Task<ActionResult<ApiResponse<bool>>> ActivateShop(int id)
            => Ok(ApiResponse<bool>.SuccessResponse(await _admin.ActivateShopAsync(id), "Shop activated"));

        /// <summary>Soft-delete a shop</summary>
        [HttpDelete("shops/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteShop(int id)
            => Ok(ApiResponse<bool>.SuccessResponse(await _admin.DeleteShopAsync(id), "Shop deleted"));

        // ── USERS ───────────────────────────────────────────────────────────────────

        /// <summary>List all users across all tenants</summary>
        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<List<AdminUserResponse>>>> GetUsers()
            => Ok(ApiResponse<List<AdminUserResponse>>.SuccessResponse(await _admin.GetAllUsersAsync(), "Users loaded"));

        /// <summary>Get a single user by ID</summary>
        [HttpGet("users/{id}")]
        public async Task<ActionResult<ApiResponse<AdminUserResponse>>> GetUser(int id)
        {
            var user = await _admin.GetUserByIdAsync(id);
            if (user == null) return NotFound(ApiResponse<object>.FailResponse("User not found"));
            return Ok(ApiResponse<AdminUserResponse>.SuccessResponse(user));
        }

        /// <summary>Activate a user account</summary>
        [HttpPost("users/{id}/activate")]
        public async Task<ActionResult<ApiResponse<bool>>> ActivateUser(int id)
            => Ok(ApiResponse<bool>.SuccessResponse(await _admin.ActivateUserAsync(id), "User activated"));

        /// <summary>Deactivate a user account</summary>
        [HttpPost("users/{id}/deactivate")]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateUser(int id)
            => Ok(ApiResponse<bool>.SuccessResponse(await _admin.DeactivateUserAsync(id), "User deactivated"));

        /// <summary>Reset a user's password</summary>
        [HttpPost("users/{id}/reset-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ResetPassword(int id, [FromQuery] string newPassword)
            => Ok(ApiResponse<bool>.SuccessResponse(await _admin.ResetUserPasswordAsync(id, newPassword), "Password reset"));

        /// <summary>Delete a user</summary>
        [HttpDelete("users/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(int id)
            => Ok(ApiResponse<bool>.SuccessResponse(await _admin.DeleteUserAsync(id), "User deleted"));

        // ── SUBSCRIPTION PLANS ──────────────────────────────────────────────────────

        /// <summary>List all subscription plans with active subscriber counts</summary>
        [HttpGet("plans")]
        public async Task<ActionResult<ApiResponse<List<AdminPlanResponse>>>> GetPlans()
            => Ok(ApiResponse<List<AdminPlanResponse>>.SuccessResponse(await _admin.GetAllPlansAsync(), "Plans loaded"));

        /// <summary>Get a single plan with full details</summary>
        [HttpGet("plans/{id}")]
        public async Task<ActionResult<ApiResponse<AdminPlanResponse>>> GetPlan(int id)
        {
            var plan = await _admin.GetPlanByIdAsync(id);
            if (plan == null) return NotFound(ApiResponse<object>.FailResponse("Plan not found"));
            return Ok(ApiResponse<AdminPlanResponse>.SuccessResponse(plan));
        }

        /// <summary>Create a new subscription plan</summary>
        [HttpPost("plans")]
        public async Task<ActionResult<ApiResponse<AdminPlanResponse>>> CreatePlan([FromBody] AdminCreatePlanRequest request)
            => Ok(ApiResponse<AdminPlanResponse>.SuccessResponse(await _admin.CreatePlanAsync(request), "Plan created"));

        /// <summary>Update an existing subscription plan</summary>
        [HttpPut("plans/{id}")]
        public async Task<ActionResult<ApiResponse<AdminPlanResponse>>> UpdatePlan(int id, [FromBody] AdminCreatePlanRequest request)
            => Ok(ApiResponse<AdminPlanResponse>.SuccessResponse(await _admin.UpdatePlanAsync(id, request), "Plan updated"));

        /// <summary>Delete (deactivate) a plan</summary>
        [HttpDelete("plans/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePlan(int id)
            => Ok(ApiResponse<bool>.SuccessResponse(await _admin.DeletePlanAsync(id), "Plan deleted"));

        // ── PLAN FEATURES ───────────────────────────────────────────────────────────

        /// <summary>List all features for a plan</summary>
        [HttpGet("plans/{planId}/features")]
        public async Task<ActionResult<ApiResponse<List<AdminPlanFeatureResponse>>>> GetPlanFeatures(int planId)
            => Ok(ApiResponse<List<AdminPlanFeatureResponse>>.SuccessResponse(await _admin.GetPlanFeaturesAsync(planId), "Features loaded"));

        /// <summary>Add a feature to a plan</summary>
        [HttpPost("plans/{planId}/features")]
        public async Task<ActionResult<ApiResponse<AdminPlanFeatureResponse>>> AddFeature(int planId, [FromBody] AdminPlanFeatureRequest request)
            => Ok(ApiResponse<AdminPlanFeatureResponse>.SuccessResponse(await _admin.AddPlanFeatureAsync(planId, request), "Feature added"));

        /// <summary>Update a plan feature</summary>
        [HttpPut("plans/features/{featureId}")]
        public async Task<ActionResult<ApiResponse<AdminPlanFeatureResponse>>> UpdateFeature(int featureId, [FromBody] AdminPlanFeatureRequest request)
            => Ok(ApiResponse<AdminPlanFeatureResponse>.SuccessResponse(await _admin.UpdatePlanFeatureAsync(featureId, request), "Feature updated"));

        /// <summary>Remove a plan feature</summary>
        [HttpDelete("plans/features/{featureId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteFeature(int featureId)
            => Ok(ApiResponse<bool>.SuccessResponse(await _admin.DeletePlanFeatureAsync(featureId), "Feature deleted"));

        // ── SUBSCRIPTIONS ───────────────────────────────────────────────────────────

        /// <summary>List all subscriptions across all tenants</summary>
        [HttpGet("subscriptions")]
        public async Task<ActionResult<ApiResponse<List<AdminSubscriptionResponse>>>> GetSubscriptions()
            => Ok(ApiResponse<List<AdminSubscriptionResponse>>.SuccessResponse(await _admin.GetAllSubscriptionsAsync(), "Subscriptions loaded"));

        /// <summary>Get a single subscription by ID</summary>
        [HttpGet("subscriptions/{id}")]
        public async Task<ActionResult<ApiResponse<AdminSubscriptionResponse>>> GetSubscription(int id)
        {
            var sub = await _admin.GetSubscriptionByIdAsync(id);
            if (sub == null) return NotFound(ApiResponse<object>.FailResponse("Subscription not found"));
            return Ok(ApiResponse<AdminSubscriptionResponse>.SuccessResponse(sub));
        }

        /// <summary>Assign / create a subscription for a shop</summary>
        [HttpPost("subscriptions")]
        public async Task<ActionResult<ApiResponse<AdminSubscriptionResponse>>> AssignSubscription([FromBody] AdminAssignSubscriptionRequest request)
            => Ok(ApiResponse<AdminSubscriptionResponse>.SuccessResponse(await _admin.AssignSubscriptionAsync(request), "Subscription assigned"));

        /// <summary>Cancel a subscription</summary>
        [HttpPost("subscriptions/{id}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelSubscription(int id, [FromQuery] string? reason)
            => Ok(ApiResponse<bool>.SuccessResponse(await _admin.CancelSubscriptionAsync(id, reason), "Subscription cancelled"));

        /// <summary>Extend a subscription by N days</summary>
        [HttpPost("subscriptions/{id}/extend")]
        public async Task<ActionResult<ApiResponse<bool>>> ExtendSubscription(int id, [FromBody] AdminExtendSubscriptionRequest request)
            => Ok(ApiResponse<bool>.SuccessResponse(await _admin.ExtendSubscriptionAsync(id, request.Days), "Subscription extended"));

        /// <summary>Update subscription status (Active / Expired / Suspended / Cancelled)</summary>
        [HttpPut("subscriptions/{id}/status")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateSubscriptionStatus(int id, [FromBody] AdminUpdateSubscriptionStatusRequest request)
            => Ok(ApiResponse<bool>.SuccessResponse(await _admin.UpdateSubscriptionStatusAsync(id, request.Status), "Status updated"));

        // ── TRIALS ──────────────────────────────────────────────────────────────────

        /// <summary>List all trials across all tenants</summary>
        [HttpGet("trials")]
        public async Task<ActionResult<ApiResponse<List<AdminTrialResponse>>>> GetTrials()
            => Ok(ApiResponse<List<AdminTrialResponse>>.SuccessResponse(await _admin.GetAllTrialsAsync(), "Trials loaded"));

        /// <summary>Get trial for a specific shop</summary>
        [HttpGet("trials/shop/{shopId}")]
        public async Task<ActionResult<ApiResponse<AdminTrialResponse>>> GetTrialForShop(int shopId)
        {
            var trial = await _admin.GetTrialByShopAsync(shopId);
            if (trial == null) return NotFound(ApiResponse<object>.FailResponse("No trial found for this shop"));
            return Ok(ApiResponse<AdminTrialResponse>.SuccessResponse(trial));
        }

        /// <summary>Start a trial for a shop (admin override)</summary>
        [HttpPost("trials/shop/{shopId}")]
        public async Task<ActionResult<ApiResponse<AdminTrialResponse>>> StartTrial(int shopId, [FromBody] AdminStartTrialRequest request)
            => Ok(ApiResponse<AdminTrialResponse>.SuccessResponse(await _admin.StartTrialForShopAsync(shopId, request.TrialDays), "Trial started"));

        /// <summary>Immediately end / expire a shop's trial</summary>
        [HttpPost("trials/shop/{shopId}/end")]
        public async Task<ActionResult<ApiResponse<bool>>> EndTrial(int shopId)
            => Ok(ApiResponse<bool>.SuccessResponse(await _admin.EndTrialAsync(shopId), "Trial ended"));

        /// <summary>Extend a shop's trial by N days</summary>
        [HttpPost("trials/shop/{shopId}/extend")]
        public async Task<ActionResult<ApiResponse<bool>>> ExtendTrial(int shopId, [FromBody] AdminExtendTrialRequest request)
            => Ok(ApiResponse<bool>.SuccessResponse(await _admin.ExtendTrialAsync(shopId, request.Days), "Trial extended"));

        // ── SUBSCRIPTION PAYMENTS ───────────────────────────────────────────────────

        /// <summary>List all subscription payments</summary>
        [HttpGet("payments")]
        public async Task<ActionResult<ApiResponse<List<AdminSubscriptionPaymentResponse>>>> GetPayments()
            => Ok(ApiResponse<List<AdminSubscriptionPaymentResponse>>.SuccessResponse(await _admin.GetAllSubscriptionPaymentsAsync(), "Payments loaded"));

        /// <summary>Manually record a subscription payment</summary>
        [HttpPost("payments")]
        public async Task<ActionResult<ApiResponse<AdminSubscriptionPaymentResponse>>> RecordPayment([FromBody] AdminRecordPaymentRequest request)
            => Ok(ApiResponse<AdminSubscriptionPaymentResponse>.SuccessResponse(await _admin.RecordSubscriptionPaymentAsync(request), "Payment recorded"));

        // ── AUDIT LOGS ──────────────────────────────────────────────────────────────

        /// <summary>Paginated audit log viewer (optionally filter by tenantId)</summary>
        [HttpGet("audit-logs")]
        public async Task<ActionResult<ApiResponse<List<AdminAuditLogResponse>>>> GetAuditLogs(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] int? tenantId = null)
            => Ok(ApiResponse<List<AdminAuditLogResponse>>.SuccessResponse(await _admin.GetAuditLogsAsync(page, pageSize, tenantId), "Audit logs loaded"));

        // ── PLAN PERMIT (SuperAdmin free grants) ─────────────────────────────────────

        /// <summary>List all businesses with their current plan permit status.</summary>
        [HttpGet("plan-permits")]
        public async Task<IActionResult> GetPlanPermits()
        {
            var permits = await _db.Subscriptions
                .Include(s => s.Business)
                .Include(s => s.Plan)
                .Where(s => s.IsGrantedBySuperAdmin)
                .OrderByDescending(s => s.GrantedAt)
                .Select(s => new
                {
                    s.Id,
                    BusinessId = s.BusinessId,
                    BusinessName = s.Business != null ? s.Business.Name : "",
                    BusinessEmail = s.Business != null ? s.Business.Email : "",
                    PlanName = s.Plan != null ? s.Plan.Name : "",
                    PlanPermit = s.PlanPermit.ToString(),
                    PermitDuration = s.PermitDuration.HasValue ? s.PermitDuration.ToString() : null,
                    Status = s.Status.ToString(),
                    s.StartDate,
                    s.EndDate,
                    s.GrantedAt,
                    s.GrantNote,
                    IsLifetime = s.Status == SubscriptionStatus.LifetimeFree
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.SuccessResponse(permits));
        }

        /// <summary>
        /// Grant a business free access (1-month or lifetime) — called by SuperAdmin.
        /// </summary>
        [HttpPost("shops/{businessId}/grant-permit")]
        public async Task<IActionResult> GrantPermit(int businessId, [FromBody] GrantPermitRequest req)
        {
            var now = DateTime.UtcNow;

            var business = await _db.Businesses.FindAsync(businessId);
            if (business == null)
                return NotFound(ApiResponse<object>.FailResponse("Business not found."));

            // Get or create the default/free plan (use lowest-price active plan)
            var plan = await _db.SubscriptionPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.MonthlyPrice)
                .FirstOrDefaultAsync();

            if (plan == null)
                return BadRequest(ApiResponse<object>.FailResponse("No active subscription plan found. Create at least one plan first."));

            // Cancel any existing active subscriptions for this business
            var existing = await _db.Subscriptions
                .Where(s => s.BusinessId == businessId &&
                            (s.Status == SubscriptionStatus.Active ||
                             s.Status == SubscriptionStatus.LifetimeFree ||
                             s.Status == SubscriptionStatus.MonthlyFree))
                .ToListAsync();

            foreach (var old in existing)
            {
                old.Status = SubscriptionStatus.Cancelled;
                old.CancelledAt = now;
                old.CancellationReason = "Superseded by SuperAdmin permit";
            }

            // Determine status and end date
            var isLifetime = req.PermitDuration == "LifetimeFree";
            var status = isLifetime ? SubscriptionStatus.LifetimeFree : SubscriptionStatus.MonthlyFree;
            var endDate = isLifetime ? DateTime.MaxValue.Date : now.AddMonths(1);

            var sub = new Subscription
            {
                BusinessId = businessId,
                PlanId = plan.Id,
                StartDate = now,
                EndDate = endDate,
                Status = status,
                AmountPaid = 0,
                AutoRenew = false,
                BillingCycle = isLifetime ? "Lifetime" : "Monthly",
                PlanPermit = PlanPermitType.Free,
                PermitDuration = isLifetime ? PlanPermitDuration.LifetimeFree : PlanPermitDuration.OneMonth,
                IsGrantedBySuperAdmin = true,
                GrantedAt = now,
                GrantNote = req.Note
            };
            _db.Subscriptions.Add(sub);
            await _db.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                sub.Id,
                BusinessId = businessId,
                BusinessName = business.Name,
                PlanPermit = "Free",
                PermitDuration = req.PermitDuration,
                Status = status.ToString(),
                sub.StartDate,
                EndDate = isLifetime ? "Lifetime" : endDate.ToString("dd-MMM-yyyy"),
                sub.GrantedAt,
                sub.GrantNote
            }, isLifetime
                ? $"✅ {business.Name} now has lifetime free access!"
                : $"✅ {business.Name} granted 1-month free access until {endDate:dd-MMM-yyyy}."));
        }

        /// <summary>Revoke SuperAdmin free permit — business reverts to normal (must pay).</summary>
        [HttpPost("shops/{businessId}/revoke-permit")]
        public async Task<IActionResult> RevokePermit(int businessId)
        {
            var now = DateTime.UtcNow;

            var activeFreePerms = await _db.Subscriptions
                .Where(s => s.BusinessId == businessId && s.IsGrantedBySuperAdmin &&
                            (s.Status == SubscriptionStatus.LifetimeFree ||
                             s.Status == SubscriptionStatus.MonthlyFree))
                .ToListAsync();

            if (!activeFreePerms.Any())
                return NotFound(ApiResponse<object>.FailResponse("No active free permit found for this business."));

            foreach (var p in activeFreePerms)
            {
                p.Status = SubscriptionStatus.Cancelled;
                p.CancelledAt = now;
                p.CancellationReason = "Revoked by SuperAdmin";
            }

            await _db.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(null, "Free permit revoked. Business must now subscribe to continue."));
        }

        /// <summary>Get plan permit status for a single business.</summary>
        [HttpGet("shops/{businessId}/permit-status")]
        public async Task<IActionResult> GetPermitStatus(int businessId)
        {
            var now = DateTime.UtcNow;
            var perm = await _db.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.BusinessId == businessId && s.IsGrantedBySuperAdmin &&
                            (s.Status == SubscriptionStatus.LifetimeFree ||
                             s.Status == SubscriptionStatus.MonthlyFree))
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (perm == null)
                return Ok(ApiResponse<object>.SuccessResponse(new { HasFreePermit = false }));

            var isLifetime = perm.Status == SubscriptionStatus.LifetimeFree;
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                HasFreePermit = true,
                PlanPermit = perm.PlanPermit.ToString(),
                PermitDuration = perm.PermitDuration?.ToString(),
                Status = perm.Status.ToString(),
                IsLifetime = isLifetime,
                perm.StartDate,
                EndDate = isLifetime ? "Lifetime" : perm.EndDate.ToString("dd-MMM-yyyy"),
                DaysRemaining = isLifetime ? -1 : Math.Max(0, (int)(perm.EndDate - now).TotalDays),
                perm.GrantedAt,
                perm.GrantNote,
                PlanName = perm.Plan?.Name
            }));
        }
    }
}
