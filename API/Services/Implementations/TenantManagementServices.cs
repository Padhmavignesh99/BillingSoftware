using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Services.Interfaces;
using UserRoleEntity = BillingSoftware.API.Models.Entities.TenantUserRole;

namespace BillingSoftware.API.Services.Implementations
{
    // =====================================================================
    // WAREHOUSE SERVICE
    // =====================================================================
    public class WarehouseService : IWarehouseService
    {
        private readonly ApplicationDbContext _db;
        public WarehouseService(ApplicationDbContext db) => _db = db;

        public async Task<List<WarehouseResponse>> GetAllAsync(int tenantId, int? branchId = null)
        {
            var q = _db.Warehouses.Include(w => w.Branch)
                        .Where(w => w.TenantId == tenantId && !w.IsDeleted);
            if (branchId.HasValue) q = q.Where(w => w.BranchId == branchId);
            return await q.Select(w => Map(w)).ToListAsync();
        }

        public async Task<WarehouseResponse?> GetByIdAsync(int id, int tenantId)
        {
            var w = await _db.Warehouses.Include(x => x.Branch)
                             .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);
            return w == null ? null : Map(w);
        }

        public async Task<WarehouseResponse> CreateAsync(CreateWarehouseRequest r, int tenantId)
        {
            // Only one default per tenant (or branch)
            if (r.IsDefault) await ClearDefaultAsync(tenantId, r.BranchId);

            var entity = new Warehouse
            {
                TenantId        = tenantId,
                Name            = r.Name,
                Code            = r.Code,
                Description     = r.Description,
                BranchId        = r.BranchId,
                Address         = r.Address,
                IsDefault       = r.IsDefault,
                StorageCapacity = r.StorageCapacity,
                CapacityUnit    = r.CapacityUnit,
                ContactPerson   = r.ContactPerson,
                ContactPhone    = r.ContactPhone
            };
            _db.Warehouses.Add(entity);
            await _db.SaveChangesAsync();
            await _db.Entry(entity).Reference(x => x.Branch).LoadAsync();
            return Map(entity);
        }

        public async Task<WarehouseResponse> UpdateAsync(int id, UpdateWarehouseRequest r, int tenantId)
        {
            var entity = await _db.Warehouses.Include(x => x.Branch)
                                  .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted)
                         ?? throw new KeyNotFoundException($"Warehouse {id} not found");

            if (r.IsDefault && !entity.IsDefault) await ClearDefaultAsync(tenantId, r.BranchId);

            entity.Name            = r.Name;
            entity.Code            = r.Code;
            entity.Description     = r.Description;
            entity.BranchId        = r.BranchId;
            entity.Address         = r.Address;
            entity.IsDefault       = r.IsDefault;
            entity.StorageCapacity = r.StorageCapacity;
            entity.CapacityUnit    = r.CapacityUnit;
            entity.ContactPerson   = r.ContactPerson;
            entity.ContactPhone    = r.ContactPhone;
            entity.UpdatedAt       = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var entity = await _db.Warehouses.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted  = true;
            entity.UpdatedAt  = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<WarehouseResponse> SetDefaultAsync(int id, int tenantId)
        {
            var entity = await _db.Warehouses.Include(x => x.Branch)
                                  .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted)
                         ?? throw new KeyNotFoundException($"Warehouse {id} not found");
            await ClearDefaultAsync(tenantId, entity.BranchId);
            entity.IsDefault = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Map(entity);
        }

        private async Task ClearDefaultAsync(int tenantId, int? branchId)
        {
            var defaults = await _db.Warehouses
                .Where(w => w.TenantId == tenantId && w.IsDefault && !w.IsDeleted &&
                            (branchId == null ? w.BranchId == null : w.BranchId == branchId))
                .ToListAsync();
            defaults.ForEach(w => { w.IsDefault = false; w.UpdatedAt = DateTime.UtcNow; });
        }

        private static WarehouseResponse Map(Warehouse w) => new()
        {
            Id              = w.Id,
            Name            = w.Name,
            Code            = w.Code,
            Description     = w.Description,
            BranchId        = w.BranchId,
            BranchName      = w.Branch?.Name,
            Address         = w.Address,
            IsDefault       = w.IsDefault,
            StorageCapacity = w.StorageCapacity,
            CapacityUnit    = w.CapacityUnit,
            ContactPerson   = w.ContactPerson,
            ContactPhone    = w.ContactPhone,
            IsActive        = w.IsActive,
            CreatedAt       = w.CreatedAt
        };
    }

    // =====================================================================
    // COUNTER SERVICE
    // =====================================================================
    public class CounterService : ICounterService
    {
        private readonly ApplicationDbContext _db;
        public CounterService(ApplicationDbContext db) => _db = db;

        public async Task<List<CounterResponse>> GetAllAsync(int tenantId, int? branchId = null)
        {
            var q = _db.Counters
                        .Include(c => c.Branch)
                        .Include(c => c.AssignedUser)
                        .Where(c => c.TenantId == tenantId && !c.IsDeleted);
            if (branchId.HasValue) q = q.Where(c => c.BranchId == branchId);
            return await q.Select(c => Map(c)).ToListAsync();
        }

        public async Task<CounterResponse?> GetByIdAsync(int id, int tenantId)
        {
            var c = await _db.Counters.Include(x => x.Branch).Include(x => x.AssignedUser)
                             .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);
            return c == null ? null : Map(c);
        }

        public async Task<CounterResponse> CreateAsync(CreateCounterRequest r, int tenantId)
        {
            if (r.IsDefault) await ClearDefaultAsync(tenantId, r.BranchId);
            var entity = new Counter
            {
                TenantId       = tenantId,
                Name           = r.Name,
                Code           = r.Code,
                BranchId       = r.BranchId,
                Description    = r.Description,
                IsDefault      = r.IsDefault,
                AssignedUserId = r.AssignedUserId,
                DeviceId       = r.DeviceId,
                IpAddress      = r.IpAddress
            };
            _db.Counters.Add(entity);
            await _db.SaveChangesAsync();
            await _db.Entry(entity).Reference(x => x.Branch).LoadAsync();
            await _db.Entry(entity).Reference(x => x.AssignedUser).LoadAsync();
            return Map(entity);
        }

        public async Task<CounterResponse> UpdateAsync(int id, UpdateCounterRequest r, int tenantId)
        {
            var entity = await _db.Counters.Include(x => x.Branch).Include(x => x.AssignedUser)
                                  .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted)
                         ?? throw new KeyNotFoundException($"Counter {id} not found");
            if (r.IsDefault && !entity.IsDefault) await ClearDefaultAsync(tenantId, r.BranchId);
            entity.Name           = r.Name;
            entity.Code           = r.Code;
            entity.BranchId       = r.BranchId;
            entity.Description    = r.Description;
            entity.IsDefault      = r.IsDefault;
            entity.AssignedUserId = r.AssignedUserId;
            entity.DeviceId       = r.DeviceId;
            entity.IpAddress      = r.IpAddress;
            entity.UpdatedAt      = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var entity = await _db.Counters.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<CounterResponse> AssignUserAsync(int counterId, int userId, int tenantId)
        {
            var entity = await _db.Counters.Include(x => x.Branch).Include(x => x.AssignedUser)
                                  .FirstOrDefaultAsync(x => x.Id == counterId && x.TenantId == tenantId && !x.IsDeleted)
                         ?? throw new KeyNotFoundException($"Counter {counterId} not found");
            entity.AssignedUserId = userId;
            entity.UpdatedAt      = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await _db.Entry(entity).Reference(x => x.AssignedUser).LoadAsync();
            return Map(entity);
        }

        public async Task<CounterResponse> SetOnlineStatusAsync(int counterId, bool isOnline, int tenantId)
        {
            var entity = await _db.Counters.Include(x => x.Branch).Include(x => x.AssignedUser)
                                  .FirstOrDefaultAsync(x => x.Id == counterId && x.TenantId == tenantId && !x.IsDeleted)
                         ?? throw new KeyNotFoundException($"Counter {counterId} not found");
            entity.IsOnline   = isOnline;
            entity.LastSeenAt = DateTime.UtcNow;
            entity.UpdatedAt  = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Map(entity);
        }

        private async Task ClearDefaultAsync(int tenantId, int branchId)
        {
            var defaults = await _db.Counters
                .Where(c => c.TenantId == tenantId && c.BranchId == branchId && c.IsDefault && !c.IsDeleted)
                .ToListAsync();
            defaults.ForEach(c => { c.IsDefault = false; c.UpdatedAt = DateTime.UtcNow; });
        }

        private static CounterResponse Map(Counter c) => new()
        {
            Id               = c.Id,
            Name             = c.Name,
            Code             = c.Code,
            BranchId         = c.BranchId,
            BranchName       = c.Branch?.Name,
            Description      = c.Description,
            IsDefault        = c.IsDefault,
            AssignedUserId   = c.AssignedUserId,
            AssignedUserName = c.AssignedUser?.FullName,
            DeviceId         = c.DeviceId,
            IpAddress        = c.IpAddress,
            IsOnline         = c.IsOnline,
            LastSeenAt       = c.LastSeenAt,
            IsActive         = c.IsActive,
            CreatedAt        = c.CreatedAt
        };
    }

    // =====================================================================
    // EMPLOYEE ROLE SERVICE
    // =====================================================================
    public class EmployeeRoleService : IEmployeeRoleService
    {
        private readonly ApplicationDbContext _db;
        public EmployeeRoleService(ApplicationDbContext db) => _db = db;

        // All system permissions defined once — extend as needed
        private static readonly List<(string Module, string Action, string Key, string Desc)> AllPermissions =
        [
            ("Dashboard",  "View",   "Dashboard.View",   "View dashboard"),
            ("Invoices",   "View",   "Invoices.View",    "View invoices"),
            ("Invoices",   "Create", "Invoices.Create",  "Create invoices"),
            ("Invoices",   "Edit",   "Invoices.Edit",    "Edit invoices"),
            ("Invoices",   "Delete", "Invoices.Delete",  "Delete invoices"),
            ("Invoices",   "Export", "Invoices.Export",  "Export invoices"),
            ("Payments",   "View",   "Payments.View",    "View payments"),
            ("Payments",   "Create", "Payments.Create",  "Record payments"),
            ("Payments",   "Refund", "Payments.Refund",  "Issue refunds"),
            ("Products",   "View",   "Products.View",    "View products"),
            ("Products",   "Create", "Products.Create",  "Add products"),
            ("Products",   "Edit",   "Products.Edit",    "Edit products"),
            ("Products",   "Delete", "Products.Delete",  "Delete products"),
            ("Inventory",  "View",   "Inventory.View",   "View stock"),
            ("Inventory",  "Adjust", "Inventory.Adjust", "Adjust stock"),
            ("Inventory",  "Transfer","Inventory.Transfer","Transfer stock"),
            ("Purchases",  "View",   "Purchases.View",   "View purchases"),
            ("Purchases",  "Create", "Purchases.Create", "Create purchase orders"),
            ("Purchases",  "Approve","Purchases.Approve","Approve purchase orders"),
            ("Customers",  "View",   "Customers.View",   "View customers"),
            ("Customers",  "Create", "Customers.Create", "Add customers"),
            ("Customers",  "Edit",   "Customers.Edit",   "Edit customers"),
            ("Suppliers",  "View",   "Suppliers.View",   "View suppliers"),
            ("Suppliers",  "Create", "Suppliers.Create", "Add suppliers"),
            ("Expenses",   "View",   "Expenses.View",    "View expenses"),
            ("Expenses",   "Create", "Expenses.Create",  "Record expenses"),
            ("Reports",    "View",   "Reports.View",     "View reports"),
            ("Reports",    "Export", "Reports.Export",   "Export reports"),
            ("Settings",   "View",   "Settings.View",    "View settings"),
            ("Settings",   "Edit",   "Settings.Edit",    "Edit settings"),
            ("Users",      "View",   "Users.View",       "View team members"),
            ("Users",      "Invite", "Users.Invite",     "Invite users"),
            ("Users",      "Edit",   "Users.Edit",       "Edit user roles"),
            ("Branches",   "View",   "Branches.View",    "View branches"),
            ("Branches",   "Manage", "Branches.Manage",  "Manage branches"),
            ("AIInsights", "View",   "AIInsights.View",  "View AI insights"),
        ];

        public async Task<List<EmployeeRoleResponse>> GetAllAsync(int tenantId)
        {
            return await _db.EmployeeRoles
                .Include(r => r.Permissions).ThenInclude(rp => rp.PermissionDef)
                .Include(r => r.UserRoles)
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .Select(r => MapRole(r))
                .ToListAsync();
        }

        public async Task<EmployeeRoleResponse?> GetByIdAsync(int id, int tenantId)
        {
            var r = await _db.EmployeeRoles
                .Include(x => x.Permissions).ThenInclude(rp => rp.PermissionDef)
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);
            return r == null ? null : MapRole(r);
        }

        public async Task<EmployeeRoleResponse> CreateAsync(CreateEmployeeRoleRequest req, int tenantId)
        {
            var role = new EmployeeRole
            {
                TenantId    = tenantId,
                Name        = req.Name,
                Description = req.Description,
                Color       = req.Color
            };
            _db.EmployeeRoles.Add(role);
            await _db.SaveChangesAsync();

            await SyncPermissionsAsync(role.Id, tenantId, req.PermissionKeys);
            return (await GetByIdAsync(role.Id, tenantId))!;
        }

        public async Task<EmployeeRoleResponse> UpdateAsync(int id, UpdateEmployeeRoleRequest req, int tenantId)
        {
            var role = await _db.EmployeeRoles
                           .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted)
                       ?? throw new KeyNotFoundException($"Role {id} not found");
            if (role.IsSystemRole) throw new InvalidOperationException("System roles cannot be modified.");
            role.Name        = req.Name;
            role.Description = req.Description;
            role.Color       = req.Color;
            role.UpdatedAt   = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await SyncPermissionsAsync(id, tenantId, req.PermissionKeys);
            return (await GetByIdAsync(id, tenantId))!;
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var role = await _db.EmployeeRoles.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);
            if (role == null) return false;
            if (role.IsSystemRole) throw new InvalidOperationException("System roles cannot be deleted.");
            role.IsDeleted = true;
            role.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<EmployeeRoleResponse> SetPermissionsAsync(int roleId, SetRolePermissionsRequest req, int tenantId)
        {
            var role = await _db.EmployeeRoles.FirstOrDefaultAsync(x => x.Id == roleId && x.TenantId == tenantId && !x.IsDeleted)
                       ?? throw new KeyNotFoundException($"Role {roleId} not found");
            await SyncPermissionsAsync(roleId, tenantId, req.PermissionKeys);
            return (await GetByIdAsync(roleId, tenantId))!;
        }

        public Task<List<PermissionGroupResponse>> GetAllPermissionsAsync()
        {
            var grouped = AllPermissions
                .GroupBy(p => p.Module)
                .Select(g => new PermissionGroupResponse
                {
                    Module = g.Key,
                    Permissions = g.Select((p, i) => new PermissionResponse
                    {
                        Module      = p.Module,
                        Action      = p.Action,
                        Key         = p.Key,
                        Description = p.Desc,
                        IsGranted   = false
                    }).ToList()
                }).ToList();
            return Task.FromResult(grouped);
        }

        public async Task<List<UserRoleResponse>> GetUsersInRoleAsync(int roleId, int tenantId)
        {
            return await _db.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Include(ur => ur.Branch)
                .Include(ur => ur.Counter)
                .Where(ur => ur.RoleId == roleId && ur.TenantId == tenantId && !ur.IsDeleted)
                .Select(ur => MapUserRole(ur))
                .ToListAsync();
        }

        // ── helpers ──────────────────────────────────────────────────────────────

        private async Task SyncPermissionsAsync(int roleId, int tenantId, List<string> keys)
        {
            // Remove existing
            var existing = await _db.RolePermissions.Include(rp => rp.PermissionDef)
                               .Where(rp => rp.RoleId == roleId).ToListAsync();
            _db.RolePermissions.RemoveRange(existing);

            // Ensure Permission records exist
            foreach (var key in keys.Distinct())
            {
                var perm = AllPermissions.FirstOrDefault(p => p.Key == key);
                if (perm == default) continue;

                var permEntity = await _db.Permissions.FirstOrDefaultAsync(p => p.Key == key);
                if (permEntity == null)
                {
                    permEntity = new Permission { Module = perm.Module, Action = perm.Action, Key = key, Description = perm.Desc };
                    _db.Permissions.Add(permEntity);
                    await _db.SaveChangesAsync();
                }
                _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permEntity.Id, IsGranted = true });
            }
            await _db.SaveChangesAsync();
        }

        private static EmployeeRoleResponse MapRole(EmployeeRole r) => new()
        {
            Id           = r.Id,
            Name         = r.Name,
            Description  = r.Description,
            Color        = r.Color,
            IsSystemRole = r.IsSystemRole,
            UserCount    = r.UserRoles.Count(ur => !ur.IsDeleted),
            IsActive     = r.IsActive,
            CreatedAt    = r.CreatedAt,
            Permissions  = r.Permissions
                            .Where(rp => rp.IsGranted)
                            .Select(rp => new PermissionResponse
                            {
                                Id          = rp.PermissionId,
                                Module      = rp.PermissionDef?.Module ?? "",
                                Action      = rp.PermissionDef?.Action ?? "",
                                Key         = rp.PermissionDef?.Key ?? "",
                                Description = rp.PermissionDef?.Description,
                                IsGranted   = rp.IsGranted
                            }).ToList()
        };

        private static UserRoleResponse MapUserRole(TenantUserRole ur) => new()
        {
            Id          = ur.Id,
            UserId      = ur.UserId,
            UserName    = ur.User?.FullName ?? "",
            UserEmail   = ur.User?.Email,
            RoleId      = ur.RoleId,
            RoleName    = ur.Role?.Name ?? "",
            RoleColor   = ur.Role?.Color,
            BranchId    = ur.BranchId,
            BranchName  = ur.Branch?.Name,
            CounterId   = ur.CounterId,
            CounterName = ur.Counter?.Name,
            ValidFrom   = ur.ValidFrom,
            ValidUntil  = ur.ValidUntil,
            IsActive    = ur.IsActive
        };
    }

    // =====================================================================
    // USER ROLE SERVICE
    // =====================================================================
    public class UserRoleService : IUserRoleService
    {
        private readonly ApplicationDbContext _db;
        public UserRoleService(ApplicationDbContext db) => _db = db;

        public async Task<List<UserRoleResponse>> GetUserRolesAsync(int userId, int tenantId)
        {
            return await _db.UserRoles
                .Include(ur => ur.User).Include(ur => ur.Role)
                .Include(ur => ur.Branch).Include(ur => ur.Counter)
                .Where(ur => ur.UserId == userId && ur.TenantId == tenantId && !ur.IsDeleted)
                .Select(ur => Map(ur)).ToListAsync();
        }

        public async Task<List<UserRoleResponse>> GetTenantUsersAsync(int tenantId)
        {
            return await _db.UserRoles
                .Include(ur => ur.User).Include(ur => ur.Role)
                .Include(ur => ur.Branch).Include(ur => ur.Counter)
                .Where(ur => ur.TenantId == tenantId && !ur.IsDeleted && ur.IsActive)
                .Select(ur => Map(ur)).ToListAsync();
        }

        public async Task<UserRoleResponse> AssignRoleAsync(AssignUserRoleRequest req, int tenantId)
        {
            // Avoid exact duplicate
            var exists = await _db.UserRoles.AnyAsync(ur =>
                ur.TenantId  == tenantId &&
                ur.UserId    == req.UserId &&
                ur.RoleId    == req.RoleId &&
                ur.BranchId  == req.BranchId &&
                ur.CounterId == req.CounterId &&
                !ur.IsDeleted);
            if (exists) throw new InvalidOperationException("This role is already assigned to the user in this scope.");

            var entity = new TenantUserRole
            {
                TenantId   = tenantId,
                UserId     = req.UserId,
                RoleId     = req.RoleId,
                BranchId   = req.BranchId,
                CounterId  = req.CounterId,
                ValidFrom  = req.ValidFrom,
                ValidUntil = req.ValidUntil
            };
            _db.UserRoles.Add(entity);
            await _db.SaveChangesAsync();
            await _db.Entry(entity).Reference(x => x.User).LoadAsync();
            await _db.Entry(entity).Reference(x => x.Role).LoadAsync();
            await _db.Entry(entity).Reference(x => x.Branch).LoadAsync();
            await _db.Entry(entity).Reference(x => x.Counter).LoadAsync();
            return Map(entity);
        }

        public async Task<bool> RevokeRoleAsync(RevokeUserRoleRequest req, int tenantId)
        {
            var entity = await _db.UserRoles.FirstOrDefaultAsync(ur =>
                ur.TenantId == tenantId && ur.UserId == req.UserId && ur.RoleId == req.RoleId && !ur.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> GetUserPermissionsAsync(int userId, int tenantId)
        {
            return await _db.UserRoles
                .Include(ur => ur.Role).ThenInclude(r => r!.Permissions).ThenInclude(rp => rp.PermissionDef)
                .Where(ur => ur.UserId == userId && ur.TenantId == tenantId && !ur.IsDeleted && ur.IsActive)
                .SelectMany(ur => ur.Role!.Permissions.Where(rp => rp.IsGranted).Select(rp => rp.PermissionDef!.Key))
                .Distinct()
                .ToListAsync();
        }

        public async Task<bool> HasPermissionAsync(int userId, int tenantId, string permissionKey)
        {
            var perms = await GetUserPermissionsAsync(userId, tenantId);
            return perms.Contains(permissionKey);
        }

        private static UserRoleResponse Map(TenantUserRole ur) => new()
        {
            Id          = ur.Id,
            UserId      = ur.UserId,
            UserName    = ur.User?.FullName ?? "",
            UserEmail   = ur.User?.Email,
            RoleId      = ur.RoleId,
            RoleName    = ur.Role?.Name ?? "",
            RoleColor   = ur.Role?.Color,
            BranchId    = ur.BranchId,
            BranchName  = ur.Branch?.Name,
            CounterId   = ur.CounterId,
            CounterName = ur.Counter?.Name,
            ValidFrom   = ur.ValidFrom,
            ValidUntil  = ur.ValidUntil,
            IsActive    = ur.IsActive
        };
    }

    // =====================================================================
    // SHOP INVITE SERVICE
    // =====================================================================
    public class ShopInviteService : IShopInviteService
    {
        private readonly ApplicationDbContext _db;
        public ShopInviteService(ApplicationDbContext db) => _db = db;

        public async Task<List<ShopInviteResponse>> GetAllAsync(int tenantId)
        {
            return await _db.ShopInvites
                .Include(i => i.Role).Include(i => i.Branch).Include(i => i.InvitedByUser)
                .Where(i => i.TenantId == tenantId && !i.IsDeleted)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => Map(i)).ToListAsync();
        }

        public async Task<ShopInviteResponse> SendInviteAsync(SendShopInviteRequest req, int tenantId, int invitedByUserId)
        {
            // Invalidate any pending invite for same email/tenant
            var pending = await _db.ShopInvites
                .Where(i => i.TenantId == tenantId && i.Email == req.Email && !i.IsAccepted && !i.IsDeleted)
                .ToListAsync();
            pending.ForEach(i => { i.IsDeleted = true; i.UpdatedAt = DateTime.UtcNow; });

            var invite = new ShopInvite
            {
                TenantId       = tenantId,
                Email          = req.Email,
                Token          = Guid.NewGuid().ToString("N"),
                RoleId         = req.RoleId,
                BranchId       = req.BranchId,
                ExpiresAt      = DateTime.UtcNow.AddDays(7),
                InvitedByUserId = invitedByUserId
            };
            _db.ShopInvites.Add(invite);
            await _db.SaveChangesAsync();
            await _db.Entry(invite).Reference(x => x.Role).LoadAsync();
            await _db.Entry(invite).Reference(x => x.Branch).LoadAsync();
            await _db.Entry(invite).Reference(x => x.InvitedByUser).LoadAsync();
            return Map(invite);
        }

        public async Task<ShopInviteResponse> AcceptInviteAsync(AcceptShopInviteRequest req)
        {
            var invite = await _db.ShopInvites
                .Include(i => i.Role).Include(i => i.Branch)
                .FirstOrDefaultAsync(i => i.Token == req.Token && !i.IsAccepted && !i.IsDeleted)
                ?? throw new KeyNotFoundException("Invite not found or already accepted.");
            if (invite.ExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("This invitation has expired.");

            invite.IsAccepted  = true;
            invite.AcceptedAt  = DateTime.UtcNow;
            invite.UpdatedAt   = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Map(invite);
        }

        public async Task<bool> CancelInviteAsync(int inviteId, int tenantId)
        {
            var invite = await _db.ShopInvites.FirstOrDefaultAsync(i => i.Id == inviteId && i.TenantId == tenantId && !i.IsDeleted);
            if (invite == null) return false;
            invite.IsDeleted = true;
            invite.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResendInviteAsync(int inviteId, int tenantId)
        {
            var invite = await _db.ShopInvites.FirstOrDefaultAsync(i => i.Id == inviteId && i.TenantId == tenantId && !i.IsDeleted);
            if (invite == null) return false;
            invite.ExpiresAt = DateTime.UtcNow.AddDays(7);
            invite.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        private static ShopInviteResponse Map(ShopInvite i) => new()
        {
            Id            = i.Id,
            Email         = i.Email,
            RoleName      = i.Role?.Name,
            BranchName    = i.Branch?.Name,
            ExpiresAt     = i.ExpiresAt,
            IsAccepted    = i.IsAccepted,
            AcceptedAt    = i.AcceptedAt,
            InvitedByName = i.InvitedByUser?.FullName ?? "",
            IsExpired     = i.ExpiresAt < DateTime.UtcNow && !i.IsAccepted,
            CreatedAt     = i.CreatedAt
        };
    }

    // =====================================================================
    // SUBSCRIPTION USAGE SERVICE
    // =====================================================================
    public class SubscriptionUsageService : ISubscriptionUsageService
    {
        private readonly ApplicationDbContext _db;
        public SubscriptionUsageService(ApplicationDbContext db) => _db = db;

        public async Task<SubscriptionUsageResponse> GetCurrentUsageAsync(int tenantId)
        {
            var now  = DateTime.UtcNow;
            var sub  = await GetActiveSubscriptionAsync(tenantId);
            var plan = sub?.Plan;
            var usage = await _db.SubscriptionUsages
                .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Month == now.Month && u.Year == now.Year);

            // If no row yet, generate live counts
            if (usage == null) await RecalculateAsync(tenantId);
            usage = await _db.SubscriptionUsages
                .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Month == now.Month && u.Year == now.Year)
                ?? new SubscriptionUsage { TenantId = tenantId, Month = now.Month, Year = now.Year };

            var alerts = await _db.UsageAlerts
                .Where(a => a.TenantId == tenantId && !a.IsAcknowledged && !a.IsDeleted)
                .Select(a => MapAlert(a)).ToListAsync();

            int maxInvoices = plan?.MaxInvoicesPerMonth ?? int.MaxValue;
            int maxProducts = plan?.MaxProducts        ?? int.MaxValue;
            int maxUsers    = plan?.MaxUsers           ?? int.MaxValue;
            int maxBranches = plan?.MaxBranches        ?? int.MaxValue;

            return new SubscriptionUsageResponse
            {
                Month           = usage.Month,
                Year            = usage.Year,
                InvoicesCreated = usage.InvoicesCreated,
                MaxInvoices     = maxInvoices,
                InvoicesPct     = maxInvoices == int.MaxValue ? 0 : (int)((double)usage.InvoicesCreated / maxInvoices * 100),
                ProductsCount   = usage.ProductsCount,
                MaxProducts     = maxProducts,
                ProductsPct     = maxProducts == int.MaxValue ? 0 : (int)((double)usage.ProductsCount / maxProducts * 100),
                UsersCount      = usage.UsersCount,
                MaxUsers        = maxUsers,
                UsersPct        = maxUsers == int.MaxValue ? 0 : (int)((double)usage.UsersCount / maxUsers * 100),
                BranchesCount   = usage.BranchesCount,
                MaxBranches     = maxBranches,
                BranchesPct     = maxBranches == int.MaxValue ? 0 : (int)((double)usage.BranchesCount / maxBranches * 100),
                ApiCallsCount   = usage.ApiCallsCount,
                PlanName        = plan?.Name ?? "None",
                SubscriptionEnd = sub?.EndDate ?? DateTime.MaxValue,
                DaysRemaining   = sub == null ? 0 : Math.Max(0, (int)(sub.EndDate - now).TotalDays),
                Alerts          = alerts
            };
        }

        public async Task<List<SubscriptionUsageResponse>> GetUsageHistoryAsync(int tenantId, int months = 6)
        {
            var now = DateTime.UtcNow;
            var result = new List<SubscriptionUsageResponse>();
            for (int i = 0; i < months; i++)
            {
                var dt = now.AddMonths(-i);
                var usage = await _db.SubscriptionUsages
                    .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Month == dt.Month && u.Year == dt.Year);
                if (usage == null) continue;
                result.Add(new SubscriptionUsageResponse
                {
                    Month           = usage.Month,
                    Year            = usage.Year,
                    InvoicesCreated = usage.InvoicesCreated,
                    ProductsCount   = usage.ProductsCount,
                    UsersCount      = usage.UsersCount,
                    BranchesCount   = usage.BranchesCount,
                    ApiCallsCount   = usage.ApiCallsCount
                });
            }
            return result;
        }

        public async Task IncrementInvoiceCountAsync(int tenantId)
        {
            var now   = DateTime.UtcNow;
            var usage = await EnsureUsageRowAsync(tenantId, now.Month, now.Year);
            usage.InvoicesCreated++;
            usage.LastUpdatedAt = now;
            await _db.SaveChangesAsync();
            await CheckThresholdsAsync(tenantId, "Invoices", usage.InvoicesCreated);
        }

        public async Task RecalculateAsync(int tenantId)
        {
            var now   = DateTime.UtcNow;
            var usage = await EnsureUsageRowAsync(tenantId, now.Month, now.Year);
            usage.InvoicesCreated = await _db.Invoices.CountAsync(i => i.TenantId == tenantId &&
                i.CreatedAt.Month == now.Month && i.CreatedAt.Year == now.Year && !i.IsDeleted);
            usage.ProductsCount   = await _db.Products.CountAsync(p => p.TenantId == tenantId && !p.IsDeleted);
            usage.UsersCount      = await _db.UserRoles.Where(ur => ur.TenantId == tenantId && !ur.IsDeleted && ur.IsActive)
                                        .Select(ur => ur.UserId).Distinct().CountAsync();
            usage.BranchesCount   = await _db.Branches.CountAsync(b => b.TenantId == tenantId && !b.IsDeleted);
            usage.LastUpdatedAt   = now;
            await _db.SaveChangesAsync();
        }

        public async Task<bool> IsWithinLimitAsync(int tenantId, string limitType)
        {
            var usage = await GetCurrentUsageAsync(tenantId);
            return limitType switch
            {
                "Invoices" => usage.InvoicesCreated < usage.MaxInvoices,
                "Products" => usage.ProductsCount   < usage.MaxProducts,
                "Users"    => usage.UsersCount      < usage.MaxUsers,
                "Branches" => usage.BranchesCount   < usage.MaxBranches,
                _          => true
            };
        }

        public async Task<List<UsageAlertResponse>> GetActiveAlertsAsync(int tenantId)
        {
            return await _db.UsageAlerts
                .Where(a => a.TenantId == tenantId && !a.IsAcknowledged && !a.IsDeleted)
                .Select(a => MapAlert(a)).ToListAsync();
        }

        public async Task AcknowledgeAlertAsync(int alertId, int tenantId, int userId)
        {
            var alert = await _db.UsageAlerts.FirstOrDefaultAsync(a => a.Id == alertId && a.TenantId == tenantId);
            if (alert == null) return;
            alert.IsAcknowledged  = true;
            alert.AcknowledgedAt  = DateTime.UtcNow;
            alert.AcknowledgedBy  = userId;
            await _db.SaveChangesAsync();
        }

        // ── helpers ──────────────────────────────────────────────────────────────

        private async Task<SubscriptionUsage> EnsureUsageRowAsync(int tenantId, int month, int year)
        {
            var usage = await _db.SubscriptionUsages
                .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Month == month && u.Year == year);
            if (usage != null) return usage;

            var sub = await GetActiveSubscriptionAsync(tenantId);
            usage = new SubscriptionUsage { TenantId = tenantId, SubscriptionId = sub?.Id ?? 0, Month = month, Year = year };
            _db.SubscriptionUsages.Add(usage);
            await _db.SaveChangesAsync();
            return usage;
        }

        private async Task<Subscription?> GetActiveSubscriptionAsync(int tenantId)
        {
            return await _db.Subscriptions.Include(s => s.Plan)
                .Where(s => s.BusinessId == tenantId && !s.IsDeleted &&
                            (s.Status.ToString() == "Active" || s.Status.ToString() == "Trial"))
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();
        }

        private async Task CheckThresholdsAsync(int tenantId, string alertType, decimal current)
        {
            var sub  = await GetActiveSubscriptionAsync(tenantId);
            if (sub?.Plan == null) return;
            decimal limit = alertType == "Invoices" ? sub.Plan.MaxInvoicesPerMonth : 0;
            if (limit <= 0) return;
            int pct = (int)(current / limit * 100);
            foreach (var threshold in new[] { 80, 90, 100 }.Where(t => pct >= t))
            {
                var existing = await _db.UsageAlerts.AnyAsync(a =>
                    a.TenantId == tenantId && a.AlertType == alertType &&
                    a.ThresholdPct == threshold && !a.IsAcknowledged && !a.IsDeleted);
                if (!existing)
                    _db.UsageAlerts.Add(new UsageAlert
                    {
                        TenantId     = tenantId,
                        AlertType    = alertType,
                        ThresholdPct = threshold,
                        CurrentValue = current,
                        LimitValue   = limit
                    });
            }
            await _db.SaveChangesAsync();
        }

        private static UsageAlertResponse MapAlert(UsageAlert a) => new()
        {
            Id            = a.Id,
            AlertType     = a.AlertType,
            ThresholdPct  = a.ThresholdPct,
            CurrentValue  = a.CurrentValue,
            LimitValue    = a.LimitValue,
            IsAcknowledged = a.IsAcknowledged,
            CreatedAt     = a.CreatedAt
        };
    }

    // =====================================================================
    // AI INSIGHT SERVICE
    // =====================================================================
    public class AIInsightService : IAIInsightService
    {
        private readonly ApplicationDbContext _db;
        public AIInsightService(ApplicationDbContext db) => _db = db;

        public async Task<List<AIInsightResponse>> GetInsightsAsync(int tenantId, string? type, bool unreadOnly, int page, int pageSize)
        {
            var q = _db.AIInsights.Where(i => i.TenantId == tenantId && !i.IsDeleted);
            if (!string.IsNullOrEmpty(type))  q = q.Where(i => i.InsightType == type);
            if (unreadOnly)                   q = q.Where(i => !i.IsRead);
            return await q.OrderByDescending(i => i.CreatedAt)
                          .Skip((page - 1) * pageSize).Take(pageSize)
                          .Select(i => Map(i)).ToListAsync();
        }

        public async Task<AIInsightResponse?> GetByIdAsync(int id, int tenantId)
        {
            var i = await _db.AIInsights.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);
            return i == null ? null : Map(i);
        }

        public async Task<AIInsightResponse> GenerateInsightAsync(GenerateInsightRequest req, int tenantId, int userId)
        {
            var periodStart = req.PeriodStart ?? DateTime.UtcNow.AddDays(-30);
            var periodEnd   = req.PeriodEnd   ?? DateTime.UtcNow;

            var (title, content, data, recommendation) = req.InsightType switch
            {
                "SalesSummary"      => await BuildSalesSummaryAsync(tenantId, periodStart, periodEnd, req.BranchId),
                "DemandForecast"    => await BuildDemandForecastAsync(tenantId, req.ProductId, req.Granularity),
                "ReorderSuggestion" => await BuildReorderSuggestionsAsync(tenantId, req.BranchId),
                "AnomalyAlert"      => await BuildAnomalyAlertAsync(tenantId, periodStart, periodEnd),
                "CustomerInsight"   => await BuildCustomerInsightAsync(tenantId, periodStart, periodEnd),
                _                   => ("Insight", "No specific generator available for this type.", null, null)
            };

            var insight = new AIInsight
            {
                TenantId            = tenantId,
                InsightType         = req.InsightType,
                Title               = title,
                Content             = content,
                DataJson            = data != null ? JsonSerializer.Serialize(data) : null,
                RecommendationJson  = recommendation != null ? JsonSerializer.Serialize(recommendation) : null,
                PeriodStart         = periodStart,
                PeriodEnd           = periodEnd,
                ConfidenceScore     = 0.85m,
                GeneratedByUserId   = userId
            };
            _db.AIInsights.Add(insight);
            await _db.SaveChangesAsync();
            return Map(insight);
        }

        public async Task<AIInsightResponse> MarkReadAsync(int id, int tenantId, MarkInsightReadRequest req)
        {
            var insight = await _db.AIInsights.FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId && !i.IsDeleted)
                          ?? throw new KeyNotFoundException($"Insight {id} not found");
            insight.IsRead    = req.IsRead;
            insight.IsActedOn = req.IsActedOn;
            insight.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Map(insight);
        }

        public async Task<bool> DeleteInsightAsync(int id, int tenantId)
        {
            var insight = await _db.AIInsights.FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId && !i.IsDeleted);
            if (insight == null) return false;
            insight.IsDeleted = true;
            insight.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(int tenantId)
            => await _db.AIInsights.CountAsync(i => i.TenantId == tenantId && !i.IsRead && !i.IsDeleted);

        public async Task<List<AIForecastResponse>> GetForecastsAsync(int tenantId, int? productId, string granularity, int days)
        {
            var from = DateTime.UtcNow;
            var to   = from.AddDays(days);
            var q = _db.AIForecasts.Include(f => f.Product)
                        .Where(f => f.TenantId == tenantId && f.Granularity == granularity &&
                                    f.ForecastDate >= from && f.ForecastDate <= to);
            if (productId.HasValue) q = q.Where(f => f.ProductId == productId.Value);
            return await q.OrderBy(f => f.ForecastDate).Select(f => MapForecast(f)).ToListAsync();
        }

        public async Task<AIForecastResponse> GenerateForecastAsync(int tenantId, int productId, string granularity, int userId)
        {
            // Simple moving-average based forecast
            var last30 = await _db.InvoiceItems
                .Include(ii => ii.Invoice)
                .Where(ii => ii.Invoice!.TenantId == tenantId && ii.ProductId == productId &&
                             !ii.Invoice.IsDeleted && ii.Invoice.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                .GroupBy(ii => ii.Invoice!.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToListAsync();

            var avgQty = last30.Count > 0 ? last30.Average(x => (double)x.Qty) : 1;
            var forecastDate = DateTime.UtcNow.Date.AddDays(1);

            var existing = await _db.AIForecasts.FirstOrDefaultAsync(f =>
                f.TenantId == tenantId && f.ProductId == productId &&
                f.ForecastDate == forecastDate && f.Granularity == granularity);

            if (existing == null)
            {
                existing = new AIForecast
                {
                    TenantId      = tenantId,
                    ProductId     = productId,
                    ForecastDate  = forecastDate,
                    Granularity   = granularity,
                    ForecastedQty = (decimal)avgQty,
                    ConfidenceLow  = (decimal)(avgQty * 0.7),
                    ConfidenceHigh = (decimal)(avgQty * 1.3),
                    ModelVersion   = "MA-30d-v1"
                };
                _db.AIForecasts.Add(existing);
            }
            else
            {
                existing.ForecastedQty = (decimal)avgQty;
                existing.UpdatedAt     = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
            await _db.Entry(existing).Reference(x => x.Product).LoadAsync();
            return MapForecast(existing);
        }

        public async Task UpdateActualQtyAsync(int tenantId, int productId, DateTime date, decimal actualQty)
        {
            var forecast = await _db.AIForecasts.FirstOrDefaultAsync(f =>
                f.TenantId == tenantId && f.ProductId == productId && f.ForecastDate == date.Date);
            if (forecast == null) return;
            forecast.ActualQty = actualQty;
            forecast.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task<object> GetSalesSummaryAsync(int tenantId, DateTime from, DateTime to, int? branchId)
        {
            var invoices = _db.Invoices.Include(i => i.Items)
                .Where(i => i.TenantId == tenantId && !i.IsDeleted &&
                            i.CreatedAt >= from && i.CreatedAt <= to);
            if (branchId.HasValue) invoices = invoices.Where(i => i.BranchId == branchId);

            var data = await invoices.ToListAsync();
            return new
            {
                TotalInvoices    = data.Count,
                TotalRevenue     = data.Sum(i => i.TotalAmount),
                TotalDiscount    = data.Sum(i => i.DiscountAmount),
                TotalTax         = data.Sum(i => i.TaxAmount),
                AverageOrderValue = data.Count > 0 ? data.Average(i => (double)i.TotalAmount) : 0,
                PeriodFrom       = from,
                PeriodTo         = to
            };
        }

        public async Task<List<object>> GetReorderSuggestionsAsync(int tenantId, int? branchId)
        {
            var products = await _db.Products
                .Where(p => p.TenantId == tenantId && !p.IsDeleted && p.IsActive &&
                            p.CurrentStock <= p.ReorderLevel)
                .Select(p => (object)new
                {
                    p.Id,
                    p.Name,
                    p.CurrentStock,
                    p.ReorderLevel,
                    SuggestedOrderQty = Math.Max(p.ReorderLevel * 2 - p.CurrentStock, 0),
                    UrgencyLevel      = p.CurrentStock == 0 ? "Critical" : p.CurrentStock <= p.ReorderLevel / 2 ? "High" : "Medium"
                }).ToListAsync();
            return products;
        }

        // ── private generators ────────────────────────────────────────────────────

        private async Task<(string Title, string Content, object? Data, object? Recommendation)>
            BuildSalesSummaryAsync(int tenantId, DateTime from, DateTime to, int? branchId)
        {
            var summary = await GetSalesSummaryAsync(tenantId, from, to, branchId);
            return ("Sales Summary", $"Sales analysis from {from:dd MMM yyyy} to {to:dd MMM yyyy}.", summary,
                new { Tip = "Consider running a promotion on slow-moving products to boost revenue." });
        }

        private async Task<(string, string, object?, object?)>
            BuildDemandForecastAsync(int tenantId, int? productId, string granularity)
        {
            var suggestions = await GetReorderSuggestionsAsync(tenantId, null);
            return ("Demand Forecast", $"Demand forecast based on the last 30 days of sales data.", suggestions,
                new { Tip = "Review reorder levels to avoid stockouts on high-demand items." });
        }

        private async Task<(string, string, object?, object?)>
            BuildReorderSuggestionsAsync(int tenantId, int? branchId)
        {
            var suggestions = await GetReorderSuggestionsAsync(tenantId, branchId);
            return ("Reorder Suggestions", $"{suggestions.Count} products are at or below reorder level.", suggestions,
                new { Tip = "Place purchase orders for critical items immediately." });
        }

        private async Task<(string, string, object?, object?)>
            BuildAnomalyAlertAsync(int tenantId, DateTime from, DateTime to)
        {
            // Simple anomaly: days with sales > 3x average
            var daily = await _db.Invoices
                .Where(i => i.TenantId == tenantId && !i.IsDeleted && i.CreatedAt >= from && i.CreatedAt <= to)
                .GroupBy(i => i.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(i => i.TotalAmount) })
                .ToListAsync();

            if (daily.Count == 0) return ("Anomaly Analysis", "No transactions found in the selected period.", null, null);
            var avg = daily.Average(d => (double)d.Revenue);
            var anomalies = daily.Where(d => (double)d.Revenue > avg * 3).ToList();
            return ("Revenue Anomaly Alert",
                anomalies.Count > 0 ? $"Found {anomalies.Count} anomalous day(s) with unusually high revenue." : "No revenue anomalies detected.",
                new { Anomalies = anomalies, DailyAverage = avg },
                new { Tip = "Investigate high-revenue days to understand what drove the spike and replicate it." });
        }

        private async Task<(string, string, object?, object?)>
            BuildCustomerInsightAsync(int tenantId, DateTime from, DateTime to)
        {
            var topCustomers = await _db.Invoices
                .Where(i => i.TenantId == tenantId && !i.IsDeleted && i.CustomerId != null &&
                            i.CreatedAt >= from && i.CreatedAt <= to)
                .GroupBy(i => i.CustomerId)
                .Select(g => new { CustomerId = g.Key, TotalSpend = g.Sum(i => i.TotalAmount), OrderCount = g.Count() })
                .OrderByDescending(x => x.TotalSpend)
                .Take(10)
                .ToListAsync();
            return ("Customer Insight", $"Top {topCustomers.Count} customers by revenue in the selected period.", topCustomers,
                new { Tip = "Engage your top customers with loyalty rewards to increase retention." });
        }

        // ── mappers ───────────────────────────────────────────────────────────────

        private static AIInsightResponse Map(AIInsight i) => new()
        {
            Id                = i.Id,
            InsightType       = i.InsightType,
            Title             = i.Title,
            Content           = i.Content,
            Data              = i.DataJson != null ? JsonSerializer.Deserialize<object>(i.DataJson) : null,
            Recommendation    = i.RecommendationJson != null ? JsonSerializer.Deserialize<object>(i.RecommendationJson) : null,
            PeriodStart       = i.PeriodStart,
            PeriodEnd         = i.PeriodEnd,
            ConfidenceScore   = i.ConfidenceScore,
            IsRead            = i.IsRead,
            IsActedOn         = i.IsActedOn,
            RelatedEntityType = i.RelatedEntityType,
            RelatedEntityId   = i.RelatedEntityId,
            GeneratedAt       = i.CreatedAt
        };

        private static AIForecastResponse MapForecast(AIForecast f) => new()
        {
            Id              = f.Id,
            ProductId       = f.ProductId,
            ProductName     = f.Product?.Name ?? "",
            ForecastDate    = f.ForecastDate,
            Granularity     = f.Granularity,
            ForecastedQty   = f.ForecastedQty,
            ActualQty       = f.ActualQty,
            ConfidenceLow   = f.ConfidenceLow,
            ConfidenceHigh  = f.ConfidenceHigh,
            ModelVersion    = f.ModelVersion
        };
    }
}
