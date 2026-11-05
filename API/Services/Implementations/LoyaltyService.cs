using MasterBilling.API.Data;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.Models.Entities;
using MasterBilling.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasterBilling.API.Services.Implementations
{
    public class LoyaltyService : ILoyaltyService
    {
        private readonly ApplicationDbContext _db;
        public LoyaltyService(ApplicationDbContext db) { _db = db; }

        public async Task<LoyaltyProgramResponse> GetProgramAsync(int tenantId)
        {
            var prog = await _db.LoyaltyPrograms.FirstOrDefaultAsync(p => p.TenantId == tenantId);
            if (prog == null)
            {
                prog = new LoyaltyProgram { TenantId = tenantId, CreatedAt = DateTime.UtcNow, IsActive = true };
                _db.LoyaltyPrograms.Add(prog);
                await _db.SaveChangesAsync();
            }
            return MapProgram(prog);
        }

        public async Task<LoyaltyProgramResponse> UpdateProgramAsync(UpdateLoyaltyProgramRequest r, int tenantId)
        {
            var prog = await _db.LoyaltyPrograms.FirstOrDefaultAsync(p => p.TenantId == tenantId);
            if (prog == null)
            {
                prog = new LoyaltyProgram { TenantId = tenantId, CreatedAt = DateTime.UtcNow, IsActive = true };
                _db.LoyaltyPrograms.Add(prog);
            }
            prog.Name = r.Name;
            prog.IsEnabled = r.IsEnabled;
            prog.PointsPerRupee = r.PointsPerRupee;
            prog.RupeePerPoint = r.RupeePerPoint;
            prog.MinRedemptionPoints = r.MinRedemptionPoints;
            prog.MaxRedemptionPercent = r.MaxRedemptionPercent;
            prog.PointsExpiryDays = r.PointsExpiryDays;
            prog.TermsAndConditions = r.TermsAndConditions;
            prog.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return MapProgram(prog);
        }

        public async Task<List<LoyaltyTierResponse>> GetTiersAsync(int tenantId)
        {
            var prog = await _db.LoyaltyPrograms.FirstOrDefaultAsync(p => p.TenantId == tenantId);
            if (prog == null) return new();
            return await _db.LoyaltyTiers
                .Where(t => t.LoyaltyProgramId == prog.Id && !t.IsDeleted)
                .OrderBy(t => t.SortOrder)
                .Select(t => new LoyaltyTierResponse
                {
                    Id = t.Id, Name = t.Name, MinPoints = t.MinPoints, MaxPoints = t.MaxPoints,
                    BonusMultiplier = t.BonusMultiplier, BadgeColor = t.BadgeColor,
                    Benefits = t.Benefits, SortOrder = t.SortOrder
                }).ToListAsync();
        }

        public async Task<LoyaltyTierResponse> CreateTierAsync(CreateLoyaltyTierRequest r, int tenantId)
        {
            var prog = await _db.LoyaltyPrograms.FirstOrDefaultAsync(p => p.TenantId == tenantId)
                ?? throw new Exception("Loyalty program not set up");
            var tier = new LoyaltyTier
            {
                TenantId = tenantId,
                LoyaltyProgramId = prog.Id,
                Name = r.Name, MinPoints = r.MinPoints, MaxPoints = r.MaxPoints,
                BonusMultiplier = r.BonusMultiplier, BadgeColor = r.BadgeColor,
                Benefits = r.Benefits, SortOrder = r.SortOrder,
                CreatedAt = DateTime.UtcNow, IsActive = true
            };
            _db.LoyaltyTiers.Add(tier);
            await _db.SaveChangesAsync();
            return new LoyaltyTierResponse { Id = tier.Id, Name = tier.Name, MinPoints = tier.MinPoints, MaxPoints = tier.MaxPoints, BonusMultiplier = tier.BonusMultiplier, BadgeColor = tier.BadgeColor, Benefits = tier.Benefits, SortOrder = tier.SortOrder };
        }

        public async Task<bool> DeleteTierAsync(int id, int tenantId)
        {
            var t = await _db.LoyaltyTiers.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
            if (t == null) return false;
            t.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<LoyaltyAccountResponse> GetCustomerAccountAsync(int customerId, int tenantId)
        {
            var prog = await _db.LoyaltyPrograms.FirstOrDefaultAsync(p => p.TenantId == tenantId);
            if (prog == null) return new LoyaltyAccountResponse { CustomerId = customerId };

            var acct = await _db.CustomerLoyaltyAccounts
                .Include(a => a.Customer)
                .Include(a => a.LoyaltyTier)
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.CustomerId == customerId && a.TenantId == tenantId);

            if (acct == null) return new LoyaltyAccountResponse { CustomerId = customerId };
            return MapAccount(acct);
        }

        public async Task<LoyaltyAccountResponse> EarnPointsAsync(EarnPointsRequest r, int tenantId)
        {
            var prog = await _db.LoyaltyPrograms.FirstOrDefaultAsync(p => p.TenantId == tenantId)
                ?? throw new Exception("Loyalty program not enabled");

            var acct = await GetOrCreateAccount(r.CustomerId, prog, tenantId);
            var tier = await _db.LoyaltyTiers.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.MinPoints <= acct.CurrentPoints);
            var multiplier = tier?.BonusMultiplier ?? 1;
            var earned = r.InvoiceAmount * prog.PointsPerRupee * multiplier;

            acct.TotalEarnedPoints += earned;
            acct.CurrentPoints += earned;
            acct.LifetimeSpend += r.InvoiceAmount;
            acct.LastActivityDate = DateTime.UtcNow;

            _db.LoyaltyTransactions.Add(new LoyaltyTransaction
            {
                TenantId = tenantId, CustomerLoyaltyAccountId = acct.Id,
                Type = "Earn", Points = earned, InvoiceAmount = r.InvoiceAmount,
                InvoiceId = r.InvoiceId, Notes = r.Notes, TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow, IsActive = true
            });

            await UpdateTier(acct, tenantId);
            await _db.SaveChangesAsync();
            return await GetCustomerAccountAsync(r.CustomerId, tenantId);
        }

        public async Task<LoyaltyAccountResponse> RedeemPointsAsync(RedeemPointsRequest r, int tenantId)
        {
            var prog = await _db.LoyaltyPrograms.FirstOrDefaultAsync(p => p.TenantId == tenantId)
                ?? throw new Exception("Loyalty program not found");
            var acct = await _db.CustomerLoyaltyAccounts.FirstOrDefaultAsync(a => a.CustomerId == r.CustomerId && a.TenantId == tenantId)
                ?? throw new Exception("Customer loyalty account not found");

            if (acct.CurrentPoints < r.Points)
                throw new Exception("Insufficient points");
            if (r.Points < prog.MinRedemptionPoints)
                throw new Exception($"Minimum redemption is {prog.MinRedemptionPoints} points");

            acct.TotalRedeemedPoints += r.Points;
            acct.CurrentPoints -= r.Points;
            acct.LastActivityDate = DateTime.UtcNow;

            _db.LoyaltyTransactions.Add(new LoyaltyTransaction
            {
                TenantId = tenantId, CustomerLoyaltyAccountId = acct.Id,
                Type = "Redeem", Points = -r.Points, InvoiceId = r.InvoiceId,
                Notes = r.Notes, TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow, IsActive = true
            });

            await _db.SaveChangesAsync();
            return await GetCustomerAccountAsync(r.CustomerId, tenantId);
        }

        public async Task<LoyaltyAccountResponse> AdjustPointsAsync(AdjustPointsRequest r, int tenantId)
        {
            var acct = await GetOrCreateAccount(r.CustomerId, null!, tenantId);
            acct.CurrentPoints += r.Points;
            if (r.Points > 0) acct.TotalEarnedPoints += r.Points;
            acct.LastActivityDate = DateTime.UtcNow;

            _db.LoyaltyTransactions.Add(new LoyaltyTransaction
            {
                TenantId = tenantId, CustomerLoyaltyAccountId = acct.Id,
                Type = "Adjust", Points = r.Points, Notes = r.Notes,
                TransactionDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, IsActive = true
            });

            await _db.SaveChangesAsync();
            return await GetCustomerAccountAsync(r.CustomerId, tenantId);
        }

        public async Task<RedeemCheckResponse> CheckRedemptionAsync(int customerId, decimal points, int tenantId)
        {
            var prog = await _db.LoyaltyPrograms.FirstOrDefaultAsync(p => p.TenantId == tenantId);
            var acct = await _db.CustomerLoyaltyAccounts.FirstOrDefaultAsync(a => a.CustomerId == customerId && a.TenantId == tenantId);

            if (prog == null || acct == null)
                return new RedeemCheckResponse { CanRedeem = false, Message = "Account not found" };

            var canRedeem = acct.CurrentPoints >= points && points >= prog.MinRedemptionPoints;
            return new RedeemCheckResponse
            {
                CanRedeem = canRedeem,
                CurrentPoints = acct.CurrentPoints,
                RequestedPoints = points,
                RedemptionValue = points * prog.RupeePerPoint,
                Message = canRedeem ? "Can redeem" : $"Insufficient points (have {acct.CurrentPoints}, need {points})"
            };
        }

        public async Task<List<LoyaltyAccountResponse>> GetTopCustomersAsync(int tenantId, int take = 10)
        {
            return await _db.CustomerLoyaltyAccounts
                .Include(a => a.Customer)
                .Include(a => a.LoyaltyTier)
                .Where(a => a.TenantId == tenantId)
                .OrderByDescending(a => a.CurrentPoints)
                .Take(take)
                .Select(a => MapAccount(a))
                .ToListAsync();
        }

        private async Task<CustomerLoyaltyAccount> GetOrCreateAccount(int customerId, LoyaltyProgram prog, int tenantId)
        {
            var acct = await _db.CustomerLoyaltyAccounts.FirstOrDefaultAsync(a => a.CustomerId == customerId && a.TenantId == tenantId);
            if (acct == null)
            {
                if (prog == null) prog = await _db.LoyaltyPrograms.FirstAsync(p => p.TenantId == tenantId);
                acct = new CustomerLoyaltyAccount
                {
                    TenantId = tenantId, CustomerId = customerId,
                    LoyaltyProgramId = prog.Id, CreatedAt = DateTime.UtcNow, IsActive = true
                };
                _db.CustomerLoyaltyAccounts.Add(acct);
                await _db.SaveChangesAsync();
            }
            return acct;
        }

        private async Task UpdateTier(CustomerLoyaltyAccount acct, int tenantId)
        {
            var tier = await _db.LoyaltyTiers
                .Where(t => t.TenantId == tenantId && t.MinPoints <= acct.CurrentPoints)
                .OrderByDescending(t => t.MinPoints)
                .FirstOrDefaultAsync();
            acct.LoyaltyTierId = tier?.Id;
        }

        private static LoyaltyProgramResponse MapProgram(LoyaltyProgram p) => new()
        {
            Id = p.Id, Name = p.Name, IsEnabled = p.IsEnabled,
            PointsPerRupee = p.PointsPerRupee, RupeePerPoint = p.RupeePerPoint,
            MinRedemptionPoints = p.MinRedemptionPoints, MaxRedemptionPercent = p.MaxRedemptionPercent,
            PointsExpiryDays = p.PointsExpiryDays, TermsAndConditions = p.TermsAndConditions
        };

        private static LoyaltyAccountResponse MapAccount(CustomerLoyaltyAccount a) => new()
        {
            Id = a.Id, CustomerId = a.CustomerId,
            CustomerName = a.Customer?.Name, CustomerPhone = a.Customer?.Phone,
            TierName = a.LoyaltyTier?.Name, TierBadgeColor = a.LoyaltyTier?.BadgeColor,
            TotalEarnedPoints = a.TotalEarnedPoints, TotalRedeemedPoints = a.TotalRedeemedPoints,
            CurrentPoints = a.CurrentPoints, LifetimeSpend = a.LifetimeSpend,
            LastActivityDate = a.LastActivityDate,
            RecentTransactions = (a.Transactions ?? new List<LoyaltyTransaction>())
                .OrderByDescending(t => t.TransactionDate).Take(10)
                .Select(t => new LoyaltyTransactionResponse
                {
                    Id = t.Id, Type = t.Type, Points = t.Points,
                    InvoiceAmount = t.InvoiceAmount, InvoiceId = t.InvoiceId,
                    Notes = t.Notes, TransactionDate = t.TransactionDate
                }).ToList()
        };
    }
}

