using MasterBilling.API.Data;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Models.Entities;
using MasterBilling.API.Models.Enums;
using MasterBilling.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasterBilling.API.Services.Implementations
{
    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;

        public TenantService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── Get all tenants ───────────────────────────────────────────────────────
        public async Task<List<TenantResponse>> GetAllTenantsAsync()
        {
            var businesses = await _context.Businesses
                .Include(b => b.BusinessType)
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var result = new List<TenantResponse>();
            foreach (var b in businesses)
            {
                var owner = await _context.Users
                    .FirstOrDefaultAsync(u => u.TenantId == b.Id && u.Role == UserRole.Owner && !u.IsDeleted);

                var subscription = await _context.Subscriptions
                    .Include(s => s.Plan)
                    .Where(s => s.BusinessId == b.Id)
                    .OrderByDescending(s => s.StartDate)
                    .FirstOrDefaultAsync();

                result.Add(MapToTenantResponse(b, owner, subscription));
            }
            return result;
        }

        // ─── Get single tenant ─────────────────────────────────────────────────────
        public async Task<TenantResponse?> GetTenantByIdAsync(int tenantId)
        {
            var b = await _context.Businesses
                .Include(b => b.BusinessType)
                .FirstOrDefaultAsync(b => b.Id == tenantId && !b.IsDeleted);

            if (b == null) return null;

            var owner = await _context.Users
                .FirstOrDefaultAsync(u => u.TenantId == b.Id && u.Role == UserRole.Owner && !u.IsDeleted);

            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.BusinessId == b.Id)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            return MapToTenantResponse(b, owner, subscription);
        }

        // ─── Stats ─────────────────────────────────────────────────────────────────
        public async Task<TenantStatsResponse> GetTenantStatsAsync(int tenantId)
        {
            var business = await _context.Businesses
                .FirstOrDefaultAsync(b => b.Id == tenantId)
                ?? throw new KeyNotFoundException($"Tenant {tenantId} not found.");

            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.BusinessId == tenantId)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            var invoicesThisMonth = await _context.Invoices
                .Where(i => i.TenantId == tenantId && !i.IsDeleted && i.CreatedAt >= startOfMonth)
                .ToListAsync();

            return new TenantStatsResponse
            {
                TenantId           = tenantId,
                TenantName         = business.Name,
                TotalUsers         = await _context.Users.CountAsync(u => u.TenantId == tenantId && !u.IsDeleted),
                TotalProducts      = await _context.Products.CountAsync(p => p.TenantId == tenantId && !p.IsDeleted),
                TotalCustomers     = await _context.Customers.CountAsync(c => c.TenantId == tenantId && !c.IsDeleted),
                TotalInvoices      = await _context.Invoices.CountAsync(i => i.TenantId == tenantId && !i.IsDeleted),
                TotalInvoicesThisMonth = invoicesThisMonth.Count,
                TotalRevenue       = await _context.Invoices
                                        .Where(i => i.TenantId == tenantId && !i.IsDeleted)
                                        .SumAsync(i => (decimal?)i.TotalAmount) ?? 0,
                RevenueThisMonth   = invoicesThisMonth.Sum(i => i.TotalAmount),
                TotalBranches      = await _context.Branches.CountAsync(b => b.TenantId == tenantId),
                ActiveSubscriptionPlan = subscription?.Plan?.Name,
                SubscriptionStatus = subscription?.Status.ToString(),
                SubscriptionExpiry = subscription?.EndDate,
                CreatedAt          = business.CreatedAt
            };
        }

        // ─── Suspend ───────────────────────────────────────────────────────────────
        public async Task<bool> SuspendTenantAsync(int tenantId)
        {
            var business = await _context.Businesses.FindAsync(tenantId)
                ?? throw new KeyNotFoundException($"Tenant {tenantId} not found.");

            business.Status = BusinessStatus.Suspended;
            business.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Activate ──────────────────────────────────────────────────────────────
        public async Task<bool> ActivateTenantAsync(int tenantId)
        {
            var business = await _context.Businesses.FindAsync(tenantId)
                ?? throw new KeyNotFoundException($"Tenant {tenantId} not found.");

            business.Status = BusinessStatus.Active;
            business.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Get users of a tenant ─────────────────────────────────────────────────
        public async Task<List<UserResponse>> GetTenantUsersAsync(int tenantId)
        {
            return await _context.Users
                .Where(u => u.TenantId == tenantId && !u.IsDeleted)
                .OrderBy(u => u.Role)
                .Select(u => new UserResponse
                {
                    Id             = u.Id,
                    FullName       = u.FullName,
                    Email          = u.Email,
                    Phone          = u.Phone,
                    Role           = u.Role.ToString(),
                    TenantId       = u.TenantId,
                    ProfileImageUrl= u.ProfileImageUrl,
                    LastLoginAt    = u.LastLoginAt,
                    IsActive       = u.IsActive
                })
                .ToListAsync();
        }

        // ─── Private mapper ────────────────────────────────────────────────────────
        private static TenantResponse MapToTenantResponse(Business b, User? owner, Subscription? subscription) =>
            new()
            {
                Id                 = b.Id,
                Name               = b.Name,
                LegalName          = b.LegalName,
                Email              = b.Email,
                Phone              = b.Phone,
                City               = b.City,
                State              = b.State,
                Country            = b.Country,
                GSTNumber          = b.GSTNumber,
                LogoUrl            = b.LogoUrl,
                Status             = b.Status.ToString(),
                BusinessTypeName   = b.BusinessType?.Name,
                CurrencyCode       = b.CurrencyCode,
                CreatedAt          = b.CreatedAt,
                IsActive           = b.IsActive,
                OwnerName          = owner?.FullName,
                OwnerEmail         = owner?.Email,
                SubscriptionPlan   = subscription?.Plan?.Name,
                SubscriptionStatus = subscription?.Status.ToString(),
                SubscriptionExpiry = subscription?.EndDate
            };
    }
}
