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
using System.Security.Claims;

namespace BillingSoftware.API.Controllers.Tenant
{
    /// <summary>
    /// User-facing subscription management.
    /// Workflow:
    ///   1. User registers → POST /api/auth/register
    ///   2. Start free trial → POST /api/subscription/start-trial
    ///   3. Check status → GET /api/subscription/status  (called on every login/app load)
    ///   4. Browse plans → GET /api/subscription/plans
    ///   5. Purchase plan → POST /api/subscription/purchase
    /// </summary>
    [ApiController]
    [Route("api/subscription")]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ITenantContext _tenant;

        public SubscriptionController(ApplicationDbContext db, ITenantContext tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        private int CurrentBusinessId => _tenant.TenantId
            ?? throw new UnauthorizedAccessException("No business associated with this account.");

        // ────────────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Check subscription/trial status for the current business.
        /// Returns IsAllowed=false when trial/subscription is expired — the frontend
        /// should redirect to the purchase page.
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<ApiResponse<SubscriptionStatusResponse>>> GetStatus()
        {
            var businessId = CurrentBusinessId;
            var now = DateTime.UtcNow;

            // Check active subscription
            var sub = await _db.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.BusinessId == businessId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (sub != null)
            {
                if (sub.Status == SubscriptionStatus.Active && sub.EndDate >= now)
                {
                    return Ok(ApiResponse<SubscriptionStatusResponse>.SuccessResponse(new SubscriptionStatusResponse
                    {
                        IsAllowed = true, Status = "Active", IsTrial = false, IsExpired = false,
                        Message = $"Active plan: {sub.Plan?.Name}. Expires on {sub.EndDate:dd-MMM-yyyy}.",
                        ExpiryDate = sub.EndDate, DaysRemaining = (int)(sub.EndDate - now).TotalDays,
                        CurrentPlanName = sub.Plan?.Name
                    }));
                }

                if (sub.Status == SubscriptionStatus.Suspended)
                {
                    return Ok(ApiResponse<SubscriptionStatusResponse>.SuccessResponse(new SubscriptionStatusResponse
                    {
                        IsAllowed = false, Status = "Suspended", IsTrial = false, IsExpired = false,
                        Message = "Your account has been suspended. Please contact support.",
                        CurrentPlanName = sub.Plan?.Name,
                        AvailablePlans = await GetPublicPlansAsync()
                    }));
                }
            }

            // Check trial
            var trial = await _db.Trials.FirstOrDefaultAsync(t => t.BusinessId == businessId);
            if (trial != null && !trial.IsConverted)
            {
                if (trial.EndDate >= now)
                {
                    return Ok(ApiResponse<SubscriptionStatusResponse>.SuccessResponse(new SubscriptionStatusResponse
                    {
                        IsAllowed = true, Status = "Trial", IsTrial = true, IsExpired = false,
                        Message = $"Free trial active. {(int)(trial.EndDate - now).TotalDays} day(s) remaining.",
                        ExpiryDate = trial.EndDate, DaysRemaining = (int)(trial.EndDate - now).TotalDays
                    }));
                }
                else
                {
                    // Trial expired — show buy page
                    return Ok(ApiResponse<SubscriptionStatusResponse>.SuccessResponse(new SubscriptionStatusResponse
                    {
                        IsAllowed = false, Status = "TrialExpired", IsTrial = true, IsExpired = true,
                        Message = "Your free trial has expired. Please purchase a plan to continue.",
                        ExpiryDate = trial.EndDate, DaysRemaining = 0,
                        AvailablePlans = await GetPublicPlansAsync()
                    }));
                }
            }

            // No subscription and no trial
            return Ok(ApiResponse<SubscriptionStatusResponse>.SuccessResponse(new SubscriptionStatusResponse
            {
                IsAllowed = false, Status = "NoSubscription", IsTrial = false, IsExpired = false,
                Message = "No active subscription or trial. Start your free 30-day trial or purchase a plan.",
                AvailablePlans = await GetPublicPlansAsync()
            }));
        }

        // ────────────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Start the 1-month free trial for the current business.
        /// Can only be called once per business.
        /// </summary>
        [HttpPost("start-trial")]
        public async Task<ActionResult<ApiResponse<AdminTrialResponse>>> StartTrial([FromBody] StartTrialRequest request)
        {
            var businessId = CurrentBusinessId;
            var existing = await _db.Trials.FirstOrDefaultAsync(t => t.BusinessId == businessId);
            if (existing != null)
                return BadRequest(ApiResponse<object>.FailResponse("A trial has already been used for this business."));

            // Get the default trial plan (Starter) or the first active plan
            var trialPlan = await _db.SubscriptionPlans.Where(p => p.IsActive).OrderBy(p => p.SortOrder).FirstOrDefaultAsync();

            var trial = new Trial
            {
                BusinessId = businessId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(trialPlan?.TrialDays ?? 30)
            };
            _db.Trials.Add(trial);

            // Update business profile details if provided
            var business = await _db.Businesses.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == businessId);
            if (business != null)
            {
                if (!string.IsNullOrEmpty(request.BusinessAddress)) business.Address = request.BusinessAddress;
                if (!string.IsNullOrEmpty(request.BusinessCity)) business.City = request.BusinessCity;
                if (!string.IsNullOrEmpty(request.BusinessState)) business.State = request.BusinessState;
                if (!string.IsNullOrEmpty(request.GSTNumber)) business.GSTNumber = request.GSTNumber;
                if (!string.IsNullOrEmpty(request.Phone) && business.Phone == null) business.Phone = request.Phone;
            }

            await _db.SaveChangesAsync();

            var now = DateTime.UtcNow;
            return Ok(ApiResponse<AdminTrialResponse>.SuccessResponse(new AdminTrialResponse
            {
                Id = trial.Id, BusinessId = trial.BusinessId, BusinessName = business?.Name ?? "",
                BusinessEmail = business?.Email, StartDate = trial.StartDate, EndDate = trial.EndDate,
                DaysRemaining = (int)(trial.EndDate - now).TotalDays, IsActive = true, IsExpired = false,
                IsConverted = false, CreatedAt = trial.CreatedAt
            }, "Free trial started! You have 30 days to explore all features."));
        }

        // ────────────────────────────────────────────────────────────────────────────
        /// <summary>
        /// List all active subscription plans (public-facing, shows pricing).
        /// </summary>
        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<SubscriptionPlanResponse>>>> GetPlans()
            => Ok(ApiResponse<List<SubscriptionPlanResponse>>.SuccessResponse(
                await GetPublicPlansAsync(), "Plans loaded"));

        // ────────────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Purchase a plan for the current business.
        /// After a successful payment, the subscription is activated.
        /// </summary>
        [HttpPost("purchase")]
        public async Task<ActionResult<ApiResponse<SubscriptionStatusResponse>>> PurchasePlan([FromBody] PurchasePlanRequest request)
        {
            var businessId = CurrentBusinessId;
            var now = DateTime.UtcNow;

            var plan = await _db.SubscriptionPlans.FindAsync(request.PlanId);
            if (plan == null || !plan.IsActive)
                return BadRequest(ApiResponse<object>.FailResponse("Invalid or inactive plan."));

            // Validate payment (in real scenario, verify with payment gateway)
            if (request.AmountPaid <= 0)
                return BadRequest(ApiResponse<object>.FailResponse("Invalid payment amount."));

            // Cancel existing active/suspended subscription
            var existing = await _db.Subscriptions.FirstOrDefaultAsync(s =>
                s.BusinessId == businessId && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Suspended));
            if (existing != null)
            {
                existing.Status = SubscriptionStatus.Cancelled;
                existing.CancelledAt = now;
                existing.CancellationReason = "Upgraded by user";
            }

            // Calculate end date based on billing cycle
            var endDate = request.BillingCycle == "Yearly"
                ? now.AddYears(1)
                : now.AddMonths(1);

            var sub = new Subscription
            {
                BusinessId = businessId, PlanId = request.PlanId,
                StartDate = now, EndDate = endDate, BillingCycle = request.BillingCycle,
                AmountPaid = request.AmountPaid, AutoRenew = true, Status = SubscriptionStatus.Active
            };
            _db.Subscriptions.Add(sub);

            // Mark trial as converted if exists
            var trial = await _db.Trials.FirstOrDefaultAsync(t => t.BusinessId == businessId && !t.IsConverted);
            if (trial != null)
            {
                trial.IsConverted = true;
                trial.ConvertedSubscriptionId = sub.Id;
            }

            // Record payment
            _db.SubscriptionPayments.Add(new SubscriptionPayment
            {
                BusinessId = businessId, SubscriptionId = sub.Id,
                Amount = request.AmountPaid, Currency = "INR",
                PaymentGateway = request.PaymentGateway ?? "Manual",
                TransactionId = request.TransactionId,
                Status = "Completed", PaymentDate = now
            });

            await _db.SaveChangesAsync();

            return Ok(ApiResponse<SubscriptionStatusResponse>.SuccessResponse(new SubscriptionStatusResponse
            {
                IsAllowed = true, Status = "Active", IsTrial = false, IsExpired = false,
                Message = $"Successfully subscribed to {plan.Name} plan! Valid until {endDate:dd-MMM-yyyy}.",
                ExpiryDate = endDate, DaysRemaining = (int)(endDate - now).TotalDays,
                CurrentPlanName = plan.Name
            }, "Subscription activated successfully!"));
        }

        // ────────────────────────────────────────────────────────────────────────────
        /// <summary>Current business subscription/trial history</summary>
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<object>>> GetHistory()
        {
            var businessId = CurrentBusinessId;
            var subs = await _db.Subscriptions.Include(s => s.Plan)
                .Where(s => s.BusinessId == businessId)
                .OrderByDescending(s => s.CreatedAt).ToListAsync();
            var payments = await _db.SubscriptionPayments
                .Where(p => p.BusinessId == businessId)
                .OrderByDescending(p => p.PaymentDate).ToListAsync();
            var trial = await _db.Trials.FirstOrDefaultAsync(t => t.BusinessId == businessId);

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                Trial = trial == null ? null : new
                {
                    trial.StartDate, trial.EndDate, trial.IsConverted,
                    DaysRemaining = Math.Max(0, (int)(trial.EndDate - DateTime.UtcNow).TotalDays)
                },
                Subscriptions = subs.Select(s => new
                {
                    s.Id, PlanName = s.Plan?.Name, Status = s.Status.ToString(), s.BillingCycle,
                    s.StartDate, s.EndDate, s.AmountPaid, s.AutoRenew,
                    DaysRemaining = Math.Max(0, (int)(s.EndDate - DateTime.UtcNow).TotalDays)
                }),
                Payments = payments.Select(p => new
                {
                    p.Id, p.Amount, p.Currency, p.PaymentGateway, p.TransactionId, p.Status, p.PaymentDate
                })
            }));
        }

        // ────────────────────────────────────────────────────────────────────────────
        private async Task<List<SubscriptionPlanResponse>> GetPublicPlansAsync()
        {
            return await _db.SubscriptionPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .Include(p => p.Features)
                .Select(p => new SubscriptionPlanResponse
                {
                    Id = p.Id, Name = p.Name, Description = p.Description,
                    MonthlyPrice = p.MonthlyPrice, YearlyPrice = p.YearlyPrice,
                    MaxProducts = p.MaxProducts, MaxUsers = p.MaxUsers,
                    MaxBranches = p.MaxBranches, MaxInvoicesPerMonth = p.MaxInvoicesPerMonth,
                    HasWhatsAppIntegration = p.HasWhatsAppIntegration, HasEmailIntegration = p.HasEmailIntegration,
                    HasReports = p.HasReports, HasMultiCurrency = p.HasMultiCurrency,
                    HasInventoryManagement = p.HasInventoryManagement,
                    IsPopular = p.IsPopular,
                    Features = p.Features.Select(f => f.FeatureName).ToList()
                })
                .ToListAsync();
        }
    }
}
