using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Models.Enums;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingSoftware.API.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _db;

        public AdminService(ApplicationDbContext db) => _db = db;

        // ────────────────────────────────────────────────────────────────────────────
        // DASHBOARD
        // ────────────────────────────────────────────────────────────────────────────
        public async Task<AdminDashboardResponse> GetDashboardAsync()
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var shops = await _db.Businesses.IgnoreQueryFilters().ToListAsync();
            var users = await _db.Users.IgnoreQueryFilters().ToListAsync();
            var trials = await _db.Trials.ToListAsync();
            var subscriptions = await _db.Subscriptions.Include(s => s.Plan).ToListAsync();
            var payments = await _db.SubscriptionPayments.ToListAsync();
            var plans = await _db.SubscriptionPlans.ToListAsync();

            var activeTrials = trials.Where(t => t.EndDate >= now && !t.IsConverted).Count();
            var expiredTrials = trials.Where(t => t.EndDate < now && !t.IsConverted).Count();
            var activeSubs = subscriptions.Where(s => s.Status == SubscriptionStatus.Active && s.EndDate >= now).Count();
            var expiredSubs = subscriptions.Where(s => s.Status == SubscriptionStatus.Expired || (s.EndDate < now && s.Status == SubscriptionStatus.Active)).Count();

            var planStats = plans.Select(p => new AdminPlanStats
            {
                PlanId = p.Id,
                PlanName = p.Name,
                ActiveSubscribers = subscriptions.Count(s => s.PlanId == p.Id && s.Status == SubscriptionStatus.Active && s.EndDate >= now),
                MonthlyRevenue = payments.Where(pay => pay.PaymentDate >= startOfMonth && subscriptions.Any(s => s.Id == pay.SubscriptionId && s.PlanId == p.Id)).Sum(pay => pay.Amount)
            }).ToList();

            var monthlyRevenue = Enumerable.Range(0, 6).Select(i =>
            {
                var month = now.AddMonths(-i);
                var start = new DateTime(month.Year, month.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var end = start.AddMonths(1);
                return new AdminMonthlyRevenue
                {
                    Month = month.ToString("MMM yyyy"),
                    Revenue = payments.Where(p => p.PaymentDate >= start && p.PaymentDate < end).Sum(p => p.Amount),
                    NewSubscriptions = subscriptions.Count(s => s.CreatedAt >= start && s.CreatedAt < end)
                };
            }).Reverse().ToList();

            return new AdminDashboardResponse
            {
                TotalShops = shops.Count(s => !s.IsDeleted),
                ActiveShops = shops.Count(s => s.Status == BusinessStatus.Active && !s.IsDeleted),
                SuspendedShops = shops.Count(s => s.Status == BusinessStatus.Suspended && !s.IsDeleted),
                TotalUsers = users.Count(u => !u.IsDeleted && u.TenantId != null),
                ActiveTrials = activeTrials,
                ExpiredTrials = expiredTrials,
                ActiveSubscriptions = activeSubs,
                ExpiredSubscriptions = expiredSubs,
                TotalRevenueThisMonth = payments.Where(p => p.PaymentDate >= startOfMonth).Sum(p => p.Amount),
                TotalRevenueAllTime = payments.Sum(p => p.Amount),
                TotalSubscriptionPlans = plans.Count(p => p.IsActive),
                PlanStats = planStats,
                MonthlyRevenue = monthlyRevenue
            };
        }

        public async Task<RevenueReportResponse> GetRevenueReportAsync()
        {
            var payments = await _db.SubscriptionPayments.Include(p => p.Business).ToListAsync();
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return new RevenueReportResponse
            {
                TotalRevenue = payments.Sum(p => p.Amount),
                MonthlyRevenue = payments.Where(p => p.PaymentDate >= startOfMonth).Sum(p => p.Amount),
                YearlyRevenue = payments.Where(p => p.PaymentDate >= startOfYear).Sum(p => p.Amount),
                ActiveSubscriptions = await _db.Subscriptions.CountAsync(s => s.Status == SubscriptionStatus.Active && s.EndDate >= now),
                SubscriptionBreakdown = await _db.SubscriptionPlans
                    .Select(p => new RevenueByPlan
                    {
                        PlanName = p.Name,
                        Count = _db.Subscriptions.Count(s => s.PlanId == p.Id && s.Status == SubscriptionStatus.Active),
                        Revenue = _db.SubscriptionPayments.Where(pay => _db.Subscriptions.Any(s => s.Id == pay.SubscriptionId && s.PlanId == p.Id)).Sum(pay => pay.Amount)
                    }).ToListAsync()
            };
        }

        // ────────────────────────────────────────────────────────────────────────────
        // SHOPS / TENANTS
        // ────────────────────────────────────────────────────────────────────────────
        public async Task<List<AdminShopResponse>> GetAllShopsAsync()
        {
            var businesses = await _db.Businesses.IgnoreQueryFilters()
                .Where(b => !b.IsDeleted)
                .Include(b => b.BusinessType)
                .ToListAsync();

            var userCounts = await _db.Users.IgnoreQueryFilters()
                .Where(u => u.TenantId != null && !u.IsDeleted)
                .GroupBy(u => u.TenantId)
                .Select(g => new { TenantId = g.Key, Count = g.Count() })
                .ToListAsync();

            var branchCounts = await _db.Branches.IgnoreQueryFilters()
                .GroupBy(b => b.TenantId)
                .Select(g => new { TenantId = g.Key, Count = g.Count() })
                .ToListAsync();

            var trials = await _db.Trials.ToListAsync();
            var subscriptions = await _db.Subscriptions.Include(s => s.Plan).ToListAsync();
            var now = DateTime.UtcNow;

            return businesses.Select(b =>
            {
                var trial = trials.FirstOrDefault(t => t.BusinessId == b.Id);
                var sub = subscriptions.Where(s => s.BusinessId == b.Id).OrderByDescending(s => s.CreatedAt).FirstOrDefault();
                string subStatus = "None";
                if (sub != null)
                    subStatus = sub.Status.ToString();
                else if (trial != null)
                    subStatus = trial.EndDate >= now ? "Trial" : "TrialExpired";

                return new AdminShopResponse
                {
                    Id = b.Id, Name = b.Name, Email = b.Email, Phone = b.Phone,
                    Address = b.Address, City = b.City, State = b.State, Country = b.Country,
                    GSTNumber = b.GSTNumber, LogoUrl = b.LogoUrl, Status = b.Status.ToString(),
                    TotalUsers = userCounts.FirstOrDefault(u => u.TenantId == b.Id)?.Count ?? 0,
                    TotalBranches = branchCounts.FirstOrDefault(br => br.TenantId == b.Id)?.Count ?? 0,
                    SubscriptionStatus = subStatus,
                    CurrentPlanName = sub?.Plan?.Name,
                    TrialStartDate = trial?.StartDate,
                    TrialEndDate = trial?.EndDate,
                    SubscriptionStartDate = sub?.StartDate,
                    SubscriptionEndDate = sub?.EndDate,
                    CreatedAt = b.CreatedAt
                };
            }).ToList();
        }

        public async Task<AdminShopResponse?> GetShopByIdAsync(int shopId)
        {
            var all = await GetAllShopsAsync();
            return all.FirstOrDefault(s => s.Id == shopId);
        }

        public async Task<bool> SuspendShopAsync(int shopId, string? reason)
        {
            var biz = await _db.Businesses.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == shopId)
                ?? throw new KeyNotFoundException($"Shop {shopId} not found.");
            biz.Status = BusinessStatus.Suspended;
            biz.SuspendedReason = reason;
            biz.SuspendedAt = DateTime.UtcNow;

            // Suspend active subscription
            var sub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.BusinessId == shopId && s.Status == SubscriptionStatus.Active);
            if (sub != null)
                sub.Status = SubscriptionStatus.Suspended;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateShopAsync(int shopId)
        {
            var biz = await _db.Businesses.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == shopId)
                ?? throw new KeyNotFoundException($"Shop {shopId} not found.");
            biz.Status = BusinessStatus.Active;
            biz.SuspendedReason = null;
            biz.SuspendedAt = null;

            // Restore suspended subscription if not expired
            var sub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.BusinessId == shopId && s.Status == SubscriptionStatus.Suspended);
            if (sub != null && sub.EndDate > DateTime.UtcNow)
                sub.Status = SubscriptionStatus.Active;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteShopAsync(int shopId)
        {
            var biz = await _db.Businesses.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == shopId)
                ?? throw new KeyNotFoundException($"Shop {shopId} not found.");
            biz.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateShopAsync(int shopId, AdminUpdateShopRequest request)
        {
            var biz = await _db.Businesses.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == shopId)
                ?? throw new KeyNotFoundException($"Shop {shopId} not found.");
            if (request.Name != null) biz.Name = request.Name;
            if (request.Email != null) biz.Email = request.Email;
            if (request.Phone != null) biz.Phone = request.Phone;
            if (request.Address != null) biz.Address = request.Address;
            if (request.City != null) biz.City = request.City;
            if (request.State != null) biz.State = request.State;
            if (request.Country != null) biz.Country = request.Country;
            if (request.GSTNumber != null) biz.GSTNumber = request.GSTNumber;
            await _db.SaveChangesAsync();
            return true;
        }

        // ────────────────────────────────────────────────────────────────────────────
        // USERS
        // ────────────────────────────────────────────────────────────────────────────
        public async Task<List<AdminUserResponse>> GetAllUsersAsync()
        {
            var users = await _db.Users.IgnoreQueryFilters()
                .Where(u => !u.IsDeleted)
                .ToListAsync();
            var businesses = await _db.Businesses.IgnoreQueryFilters().ToListAsync();

            return users.Select(u => new AdminUserResponse
            {
                Id = u.Id, FullName = u.FullName, Email = u.Email, Phone = u.Phone,
                Role = u.Role.ToString(), TenantId = u.TenantId,
                BusinessName = u.TenantId.HasValue ? businesses.FirstOrDefault(b => b.Id == u.TenantId)?.Name : null,
                IsActive = u.IsActive, LastLoginAt = u.LastLoginAt, CreatedAt = u.CreatedAt
            }).ToList();
        }

        public async Task<AdminUserResponse?> GetUserByIdAsync(int userId)
        {
            var all = await GetAllUsersAsync();
            return all.FirstOrDefault(u => u.Id == userId);
        }

        public async Task<bool> ActivateUserAsync(int userId)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException($"User {userId} not found.");
            user.IsActive = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException($"User {userId} not found.");
            user.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetUserPasswordAsync(int userId, string newPassword)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException($"User {userId} not found.");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException($"User {userId} not found.");
            user.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }

        // ────────────────────────────────────────────────────────────────────────────
        // SUBSCRIPTION PLANS
        // ────────────────────────────────────────────────────────────────────────────
        private async Task<AdminPlanResponse> MapPlanAsync(SubscriptionPlan p)
        {
            var now = DateTime.UtcNow;
            var activeCount = await _db.Subscriptions.CountAsync(s => s.PlanId == p.Id && s.Status == SubscriptionStatus.Active && s.EndDate >= now);
            var features = await _db.PlanFeatures.Where(f => f.PlanId == p.Id).ToListAsync();
            return new AdminPlanResponse
            {
                Id = p.Id, Name = p.Name, Description = p.Description,
                MonthlyPrice = p.MonthlyPrice, YearlyPrice = p.YearlyPrice,
                MaxProducts = p.MaxProducts, MaxUsers = p.MaxUsers, MaxBranches = p.MaxBranches,
                MaxInvoicesPerMonth = p.MaxInvoicesPerMonth,
                HasWhatsAppIntegration = p.HasWhatsAppIntegration, HasEmailIntegration = p.HasEmailIntegration,
                HasReports = p.HasReports, HasMultiCurrency = p.HasMultiCurrency,
                HasInventoryManagement = p.HasInventoryManagement, HasDeviceIntegration = p.HasDeviceIntegration,
                HasApiAccess = p.HasApiAccess, TrialDays = p.TrialDays,
                IsActive = p.IsActive, IsPopular = p.IsPopular, SortOrder = p.SortOrder,
                ActiveSubscribers = activeCount, CreatedAt = p.CreatedAt,
                Features = features.Select(f => new AdminPlanFeatureResponse
                {
                    Id = f.Id, PlanId = f.PlanId, FeatureName = f.FeatureName,
                    Description = f.Description, IsEnabled = f.IsEnabled, FeatureValue = f.FeatureValue
                }).ToList()
            };
        }

        public async Task<List<AdminPlanResponse>> GetAllPlansAsync()
        {
            var plans = await _db.SubscriptionPlans.OrderBy(p => p.SortOrder).ToListAsync();
            var results = new List<AdminPlanResponse>();
            foreach (var p in plans)
                results.Add(await MapPlanAsync(p));
            return results;
        }

        public async Task<AdminPlanResponse?> GetPlanByIdAsync(int planId)
        {
            var p = await _db.SubscriptionPlans.FindAsync(planId);
            return p == null ? null : await MapPlanAsync(p);
        }

        public async Task<AdminPlanResponse> CreatePlanAsync(AdminCreatePlanRequest request)
        {
            var plan = new SubscriptionPlan
            {
                Name = request.Name, Description = request.Description,
                MonthlyPrice = request.MonthlyPrice, YearlyPrice = request.YearlyPrice,
                MaxProducts = request.MaxProducts, MaxUsers = request.MaxUsers,
                MaxBranches = request.MaxBranches, MaxInvoicesPerMonth = request.MaxInvoicesPerMonth,
                HasWhatsAppIntegration = request.HasWhatsAppIntegration, HasEmailIntegration = request.HasEmailIntegration,
                HasReports = request.HasReports, HasMultiCurrency = request.HasMultiCurrency,
                HasInventoryManagement = request.HasInventoryManagement, HasDeviceIntegration = request.HasDeviceIntegration,
                HasApiAccess = request.HasApiAccess, TrialDays = request.TrialDays,
                IsActive = request.IsActive, IsPopular = request.IsPopular, SortOrder = request.SortOrder
            };
            _db.SubscriptionPlans.Add(plan);
            await _db.SaveChangesAsync();
            return await MapPlanAsync(plan);
        }

        public async Task<AdminPlanResponse> UpdatePlanAsync(int planId, AdminCreatePlanRequest request)
        {
            var plan = await _db.SubscriptionPlans.FindAsync(planId)
                ?? throw new KeyNotFoundException($"Plan {planId} not found.");
            plan.Name = request.Name; plan.Description = request.Description;
            plan.MonthlyPrice = request.MonthlyPrice; plan.YearlyPrice = request.YearlyPrice;
            plan.MaxProducts = request.MaxProducts; plan.MaxUsers = request.MaxUsers;
            plan.MaxBranches = request.MaxBranches; plan.MaxInvoicesPerMonth = request.MaxInvoicesPerMonth;
            plan.HasWhatsAppIntegration = request.HasWhatsAppIntegration; plan.HasEmailIntegration = request.HasEmailIntegration;
            plan.HasReports = request.HasReports; plan.HasMultiCurrency = request.HasMultiCurrency;
            plan.HasInventoryManagement = request.HasInventoryManagement; plan.HasDeviceIntegration = request.HasDeviceIntegration;
            plan.HasApiAccess = request.HasApiAccess; plan.TrialDays = request.TrialDays;
            plan.IsActive = request.IsActive; plan.IsPopular = request.IsPopular; plan.SortOrder = request.SortOrder;
            await _db.SaveChangesAsync();
            return await MapPlanAsync(plan);
        }

        public async Task<bool> DeletePlanAsync(int planId)
        {
            var plan = await _db.SubscriptionPlans.FindAsync(planId)
                ?? throw new KeyNotFoundException($"Plan {planId} not found.");
            plan.IsDeleted = true;
            plan.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        // ────────────────────────────────────────────────────────────────────────────
        // PLAN FEATURES
        // ────────────────────────────────────────────────────────────────────────────
        public async Task<List<AdminPlanFeatureResponse>> GetPlanFeaturesAsync(int planId)
        {
            return await _db.PlanFeatures.Where(f => f.PlanId == planId).Select(f => new AdminPlanFeatureResponse
            {
                Id = f.Id, PlanId = f.PlanId, FeatureName = f.FeatureName,
                Description = f.Description, IsEnabled = f.IsEnabled, FeatureValue = f.FeatureValue
            }).ToListAsync();
        }

        public async Task<AdminPlanFeatureResponse> AddPlanFeatureAsync(int planId, AdminPlanFeatureRequest request)
        {
            var feature = new PlanFeature
            {
                PlanId = planId, FeatureName = request.FeatureName, Description = request.Description,
                IsEnabled = request.IsEnabled, FeatureValue = request.FeatureValue
            };
            _db.PlanFeatures.Add(feature);
            await _db.SaveChangesAsync();
            return new AdminPlanFeatureResponse
            {
                Id = feature.Id, PlanId = feature.PlanId, FeatureName = feature.FeatureName,
                Description = feature.Description, IsEnabled = feature.IsEnabled, FeatureValue = feature.FeatureValue
            };
        }

        public async Task<AdminPlanFeatureResponse> UpdatePlanFeatureAsync(int featureId, AdminPlanFeatureRequest request)
        {
            var feature = await _db.PlanFeatures.FindAsync(featureId)
                ?? throw new KeyNotFoundException($"Feature {featureId} not found.");
            feature.FeatureName = request.FeatureName; feature.Description = request.Description;
            feature.IsEnabled = request.IsEnabled; feature.FeatureValue = request.FeatureValue;
            await _db.SaveChangesAsync();
            return new AdminPlanFeatureResponse
            {
                Id = feature.Id, PlanId = feature.PlanId, FeatureName = feature.FeatureName,
                Description = feature.Description, IsEnabled = feature.IsEnabled, FeatureValue = feature.FeatureValue
            };
        }

        public async Task<bool> DeletePlanFeatureAsync(int featureId)
        {
            var feature = await _db.PlanFeatures.FindAsync(featureId)
                ?? throw new KeyNotFoundException($"Feature {featureId} not found.");
            feature.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }

        // ────────────────────────────────────────────────────────────────────────────
        // SUBSCRIPTIONS
        // ────────────────────────────────────────────────────────────────────────────
        private AdminSubscriptionResponse MapSubscription(Subscription s, Business? biz, SubscriptionPlan? plan)
        {
            var now = DateTime.UtcNow;
            return new AdminSubscriptionResponse
            {
                Id = s.Id, BusinessId = s.BusinessId, BusinessName = biz?.Name ?? "Unknown",
                BusinessEmail = biz?.Email, PlanId = s.PlanId,
                PlanName = plan?.Name ?? "Unknown", PlanMonthlyPrice = plan?.MonthlyPrice ?? 0,
                Status = s.Status.ToString(), BillingCycle = s.BillingCycle ?? "Monthly",
                StartDate = s.StartDate, EndDate = s.EndDate,
                DaysRemaining = Math.Max(0, (int)(s.EndDate - now).TotalDays),
                AmountPaid = s.AmountPaid, AutoRenew = s.AutoRenew,
                CancelledAt = s.CancelledAt, CancellationReason = s.CancellationReason,
                CreatedAt = s.CreatedAt
            };
        }

        public async Task<List<AdminSubscriptionResponse>> GetAllSubscriptionsAsync()
        {
            var subs = await _db.Subscriptions.Include(s => s.Plan).Include(s => s.Business).ToListAsync();
            return subs.Select(s => MapSubscription(s, s.Business, s.Plan)).ToList();
        }

        public async Task<AdminSubscriptionResponse?> GetSubscriptionByIdAsync(int id)
        {
            var s = await _db.Subscriptions.Include(s => s.Plan).Include(s => s.Business).FirstOrDefaultAsync(s => s.Id == id);
            return s == null ? null : MapSubscription(s, s.Business, s.Plan);
        }

        public async Task<AdminSubscriptionResponse> AssignSubscriptionAsync(AdminAssignSubscriptionRequest request)
        {
            var biz = await _db.Businesses.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == request.BusinessId)
                ?? throw new KeyNotFoundException($"Business {request.BusinessId} not found.");
            var plan = await _db.SubscriptionPlans.FindAsync(request.PlanId)
                ?? throw new KeyNotFoundException($"Plan {request.PlanId} not found.");

            // Expire current active/trial subscription
            var existing = await _db.Subscriptions.FirstOrDefaultAsync(s =>
                s.BusinessId == request.BusinessId &&
                (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial || s.Status == SubscriptionStatus.Suspended));
            if (existing != null)
            {
                existing.Status = SubscriptionStatus.Cancelled;
                existing.CancelledAt = DateTime.UtcNow;
                existing.CancellationReason = "Replaced by admin assignment";
            }

            var sub = new Subscription
            {
                BusinessId = request.BusinessId, PlanId = request.PlanId,
                StartDate = request.StartDate, EndDate = request.EndDate,
                BillingCycle = request.BillingCycle, AmountPaid = request.AmountPaid,
                AutoRenew = request.AutoRenew, Status = SubscriptionStatus.Active
            };
            _db.Subscriptions.Add(sub);

            // Mark trial as converted if exists
            var trial = await _db.Trials.FirstOrDefaultAsync(t => t.BusinessId == request.BusinessId && !t.IsConverted);
            if (trial != null)
            {
                trial.IsConverted = true;
                trial.ConvertedSubscriptionId = sub.Id;
            }

            if (request.AmountPaid > 0)
            {
                _db.SubscriptionPayments.Add(new SubscriptionPayment
                {
                    BusinessId = request.BusinessId, SubscriptionId = sub.Id,
                    Amount = request.AmountPaid, Status = "Completed",
                    PaymentDate = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
            return MapSubscription(sub, biz, plan);
        }

        public async Task<bool> CancelSubscriptionAsync(int subscriptionId, string? reason)
        {
            var sub = await _db.Subscriptions.FindAsync(subscriptionId)
                ?? throw new KeyNotFoundException($"Subscription {subscriptionId} not found.");
            sub.Status = SubscriptionStatus.Cancelled;
            sub.CancelledAt = DateTime.UtcNow;
            sub.CancellationReason = reason;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExtendSubscriptionAsync(int subscriptionId, int days)
        {
            var sub = await _db.Subscriptions.FindAsync(subscriptionId)
                ?? throw new KeyNotFoundException($"Subscription {subscriptionId} not found.");
            sub.EndDate = sub.EndDate.AddDays(days);
            if (sub.Status == SubscriptionStatus.Expired && sub.EndDate > DateTime.UtcNow)
                sub.Status = SubscriptionStatus.Active;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSubscriptionStatusAsync(int subscriptionId, string status)
        {
            var sub = await _db.Subscriptions.FindAsync(subscriptionId)
                ?? throw new KeyNotFoundException($"Subscription {subscriptionId} not found.");
            sub.Status = Enum.Parse<SubscriptionStatus>(status, true);
            await _db.SaveChangesAsync();
            return true;
        }

        // ────────────────────────────────────────────────────────────────────────────
        // TRIALS
        // ────────────────────────────────────────────────────────────────────────────
        private AdminTrialResponse MapTrial(Trial t, Business? biz, SubscriptionPlan? convertedPlan)
        {
            var now = DateTime.UtcNow;
            return new AdminTrialResponse
            {
                Id = t.Id, BusinessId = t.BusinessId, BusinessName = biz?.Name ?? "Unknown",
                BusinessEmail = biz?.Email, StartDate = t.StartDate, EndDate = t.EndDate,
                DaysRemaining = Math.Max(0, (int)(t.EndDate - now).TotalDays),
                IsActive = t.EndDate >= now && !t.IsConverted,
                IsExpired = t.EndDate < now && !t.IsConverted,
                IsConverted = t.IsConverted, ConvertedPlanName = convertedPlan?.Name,
                CreatedAt = t.CreatedAt
            };
        }

        public async Task<List<AdminTrialResponse>> GetAllTrialsAsync()
        {
            var trials = await _db.Trials.Include(t => t.Business).ToListAsync();
            var convertedSubs = await _db.Subscriptions.Include(s => s.Plan).ToListAsync();
            return trials.Select(t =>
            {
                var convertedPlan = t.ConvertedSubscriptionId.HasValue
                    ? convertedSubs.FirstOrDefault(s => s.Id == t.ConvertedSubscriptionId)?.Plan
                    : null;
                return MapTrial(t, t.Business, convertedPlan);
            }).ToList();
        }

        public async Task<AdminTrialResponse?> GetTrialByShopAsync(int shopId)
        {
            var t = await _db.Trials.Include(t => t.Business).FirstOrDefaultAsync(t => t.BusinessId == shopId);
            if (t == null) return null;
            var convertedPlan = t.ConvertedSubscriptionId.HasValue
                ? (await _db.Subscriptions.Include(s => s.Plan).FirstOrDefaultAsync(s => s.Id == t.ConvertedSubscriptionId))?.Plan
                : null;
            return MapTrial(t, t.Business, convertedPlan);
        }

        public async Task<AdminTrialResponse> StartTrialForShopAsync(int shopId, int trialDays)
        {
            var biz = await _db.Businesses.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == shopId)
                ?? throw new KeyNotFoundException($"Business {shopId} not found.");
            var existing = await _db.Trials.FirstOrDefaultAsync(t => t.BusinessId == shopId);
            if (existing != null)
                throw new InvalidOperationException("Trial already exists for this business.");

            var trial = new Trial
            {
                BusinessId = shopId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(trialDays)
            };
            _db.Trials.Add(trial);
            await _db.SaveChangesAsync();
            return MapTrial(trial, biz, null);
        }

        public async Task<bool> EndTrialAsync(int shopId)
        {
            var trial = await _db.Trials.FirstOrDefaultAsync(t => t.BusinessId == shopId)
                ?? throw new KeyNotFoundException($"No trial found for shop {shopId}.");
            trial.EndDate = DateTime.UtcNow.AddSeconds(-1); // immediately expired
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExtendTrialAsync(int shopId, int days)
        {
            var trial = await _db.Trials.FirstOrDefaultAsync(t => t.BusinessId == shopId)
                ?? throw new KeyNotFoundException($"No trial found for shop {shopId}.");
            // If trial already expired, extend from now; otherwise from EndDate
            var baseDate = trial.EndDate < DateTime.UtcNow ? DateTime.UtcNow : trial.EndDate;
            trial.EndDate = baseDate.AddDays(days);
            await _db.SaveChangesAsync();
            return true;
        }

        // ────────────────────────────────────────────────────────────────────────────
        // SUBSCRIPTION PAYMENTS
        // ────────────────────────────────────────────────────────────────────────────
        public async Task<List<AdminSubscriptionPaymentResponse>> GetAllSubscriptionPaymentsAsync()
        {
            var payments = await _db.SubscriptionPayments.Include(p => p.Business).OrderByDescending(p => p.PaymentDate).ToListAsync();
            return payments.Select(p => new AdminSubscriptionPaymentResponse
            {
                Id = p.Id, SubscriptionId = p.SubscriptionId, BusinessId = p.BusinessId,
                BusinessName = p.Business?.Name ?? "Unknown", Amount = p.Amount, Currency = p.Currency,
                PaymentGateway = p.PaymentGateway, TransactionId = p.TransactionId,
                Status = p.Status, PaymentDate = p.PaymentDate, CreatedAt = p.CreatedAt
            }).ToList();
        }

        public async Task<AdminSubscriptionPaymentResponse> RecordSubscriptionPaymentAsync(AdminRecordPaymentRequest request)
        {
            var biz = await _db.Businesses.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == request.BusinessId)
                ?? throw new KeyNotFoundException($"Business {request.BusinessId} not found.");

            var payment = new SubscriptionPayment
            {
                BusinessId = request.BusinessId, SubscriptionId = request.SubscriptionId,
                Amount = request.Amount, Currency = request.Currency ?? "INR",
                PaymentGateway = request.PaymentGateway, TransactionId = request.TransactionId,
                Status = request.Status, PaymentDate = request.PaymentDate ?? DateTime.UtcNow
            };
            _db.SubscriptionPayments.Add(payment);
            await _db.SaveChangesAsync();

            return new AdminSubscriptionPaymentResponse
            {
                Id = payment.Id, SubscriptionId = payment.SubscriptionId, BusinessId = payment.BusinessId,
                BusinessName = biz.Name, Amount = payment.Amount, Currency = payment.Currency,
                PaymentGateway = payment.PaymentGateway, TransactionId = payment.TransactionId,
                Status = payment.Status, PaymentDate = payment.PaymentDate, CreatedAt = payment.CreatedAt
            };
        }

        // ────────────────────────────────────────────────────────────────────────────
        // AUDIT LOGS
        // ────────────────────────────────────────────────────────────────────────────
        public async Task<List<AdminAuditLogResponse>> GetAuditLogsAsync(int page, int pageSize, int? tenantId)
        {
            var query = _db.AuditLogs.AsQueryable();
            if (tenantId.HasValue)
                query = query.Where(l => l.TenantId == tenantId);

            var logs = await query.OrderByDescending(l => l.Timestamp).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var userIds = logs.Where(l => l.UserId.HasValue).Select(l => l.UserId!.Value).Distinct().ToList();
            var tenantIds = logs.Where(l => l.TenantId.HasValue).Select(l => l.TenantId!.Value).Distinct().ToList();
            var users = await _db.Users.IgnoreQueryFilters().Where(u => userIds.Contains(u.Id)).ToListAsync();
            var bizNames = await _db.Businesses.IgnoreQueryFilters().Where(b => tenantIds.Contains(b.Id)).ToDictionaryAsync(b => b.Id, b => b.Name);

            return logs.Select(l => new AdminAuditLogResponse
            {
                Id = l.Id, TenantId = l.TenantId,
                BusinessName = l.TenantId.HasValue && bizNames.ContainsKey(l.TenantId.Value) ? bizNames[l.TenantId.Value] : null,
                UserId = l.UserId,
                UserName = l.UserId.HasValue ? users.FirstOrDefault(u => u.Id == l.UserId)?.FullName : null,
                Action = l.Action, EntityType = l.EntityType, EntityId = l.EntityId,
                OldValues = l.OldValues, NewValues = l.NewValues, IpAddress = l.IpAddress,
                Timestamp = l.Timestamp
            }).ToList();
        }
    }
}
