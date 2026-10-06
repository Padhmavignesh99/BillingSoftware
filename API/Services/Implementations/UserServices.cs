using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Models.Enums;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingSoftware.API.Services.Implementations
{
    // ==================== USER SERVICE ====================
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        public UserService(ApplicationDbContext context) { _context = context; }

        public async Task<List<UserResponse>> GetAllUsersAsync(int tenantId)
        {
            return await _context.Users.Where(u => u.TenantId == tenantId && !u.IsDeleted)
                .Select(u => MapToResponse(u)).ToListAsync();
        }

        public async Task<UserResponse?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? null : MapToResponse(user);
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, int tenantId)
        {
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                TenantId = tenantId
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return MapToResponse(user);
        }

        public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id) ?? throw new KeyNotFoundException("User not found");
            if (request.FullName != null) user.FullName = request.FullName;
            if (request.Phone != null) user.Phone = request.Phone;
            if (request.ProfileImageUrl != null) user.ProfileImageUrl = request.ProfileImageUrl;
            await _context.SaveChangesAsync();
            return MapToResponse(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id) ?? throw new KeyNotFoundException("User not found");
            user.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserResponse> UpdateUserRoleAsync(int id, UpdateUserRoleRequest request)
        {
            var user = await _context.Users.FindAsync(id) ?? throw new KeyNotFoundException("User not found");
            user.Role = request.Role;
            await _context.SaveChangesAsync();
            return MapToResponse(user);
        }

        private static UserResponse MapToResponse(User u) => new()
        {
            Id = u.Id, FullName = u.FullName, Email = u.Email, Phone = u.Phone,
            Role = u.Role.ToString(), TenantId = u.TenantId, ProfileImageUrl = u.ProfileImageUrl,
            LastLoginAt = u.LastLoginAt, IsActive = u.IsActive
        };
    }

    // ==================== ROLE SERVICE ====================
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;
        public RoleService(ApplicationDbContext context) { _context = context; }

        public async Task<List<RoleResponse>> GetAllRolesAsync() =>
            await _context.Roles.Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .Select(r => new RoleResponse
                {
                    Id = r.Id, Name = r.Name, Description = r.Description,
                    Permissions = r.RolePermissions.Select(rp => new PermissionResponse
                    { Id = rp.Permission!.Id, Name = rp.Permission.Name, Module = rp.Permission.Module, Description = rp.Permission.Description }).ToList()
                }).ToListAsync();

        public async Task<RoleResponse?> GetRoleByIdAsync(int id)
        {
            var r = await _context.Roles.Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission).FirstOrDefaultAsync(r => r.Id == id);
            if (r == null) return null;
            return new RoleResponse { Id = r.Id, Name = r.Name, Description = r.Description,
                Permissions = r.RolePermissions.Select(rp => new PermissionResponse { Id = rp.Permission!.Id, Name = rp.Permission.Name, Module = rp.Permission.Module }).ToList() };
        }

        public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request)
        {
            var role = new Role { Name = request.Name, Description = request.Description };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            if (request.PermissionIds?.Any() == true)
            {
                foreach (var pid in request.PermissionIds)
                    _context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = pid });
                await _context.SaveChangesAsync();
            }
            return (await GetRoleByIdAsync(role.Id))!;
        }

        public async Task<RoleResponse> UpdateRoleAsync(int id, UpdateRoleRequest request)
        {
            var role = await _context.Roles.FindAsync(id) ?? throw new KeyNotFoundException("Role not found");
            if (request.Name != null) role.Name = request.Name;
            if (request.Description != null) role.Description = request.Description;
            if (request.PermissionIds != null)
            {
                var existing = _context.RolePermissions.Where(rp => rp.RoleId == id);
                _context.RolePermissions.RemoveRange(existing);
                foreach (var pid in request.PermissionIds)
                    _context.RolePermissions.Add(new RolePermission { RoleId = id, PermissionId = pid });
            }
            await _context.SaveChangesAsync();
            return (await GetRoleByIdAsync(id))!;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id) ?? throw new KeyNotFoundException("Role not found");
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    // ==================== PERMISSION SERVICE ====================
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;
        public PermissionService(ApplicationDbContext context) { _context = context; }

        public async Task<List<PermissionResponse>> GetAllPermissionsAsync() =>
            await _context.Permissions.Select(p => new PermissionResponse { Id = p.Id, Name = p.Name, Module = p.Module, Description = p.Description }).ToListAsync();

        public async Task<PermissionResponse> CreatePermissionAsync(CreatePermissionRequest request)
        {
            var perm = new Permission { Name = request.Name, Module = request.Module, Description = request.Description };
            _context.Permissions.Add(perm);
            await _context.SaveChangesAsync();
            return new PermissionResponse { Id = perm.Id, Name = perm.Name, Module = perm.Module, Description = perm.Description };
        }

        public async Task<bool> DeletePermissionAsync(int id)
        {
            var perm = await _context.Permissions.FindAsync(id) ?? throw new KeyNotFoundException("Permission not found");
            _context.Permissions.Remove(perm);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    // ==================== STAFF SERVICE ====================
    public class StaffService : IStaffService
    {
        private readonly ApplicationDbContext _context;
        public StaffService(ApplicationDbContext context) { _context = context; }

        public async Task<List<StaffResponse>> GetAllStaffAsync(int tenantId) =>
            await _context.Staff.Include(s => s.User).Include(s => s.Branch).Where(s => s.TenantId == tenantId && !s.IsDeleted)
                .Select(s => new StaffResponse { Id = s.Id, Department = s.Department, Designation = s.Designation, Salary = s.Salary, JoiningDate = s.JoiningDate, BranchId = s.BranchId, BranchName = s.Branch != null ? s.Branch.Name : null,
                    User = new UserResponse { Id = s.User!.Id, FullName = s.User.FullName, Email = s.User.Email, Phone = s.User.Phone, Role = s.User.Role.ToString() } }).ToListAsync();

        public async Task<StaffResponse?> GetStaffByIdAsync(int id)
        {
            var s = await _context.Staff.Include(s => s.User).Include(s => s.Branch).FirstOrDefaultAsync(s => s.Id == id);
            if (s == null) return null;
            return new StaffResponse { Id = s.Id, Department = s.Department, Designation = s.Designation, Salary = s.Salary, JoiningDate = s.JoiningDate, BranchId = s.BranchId, BranchName = s.Branch?.Name,
                User = new UserResponse { Id = s.User!.Id, FullName = s.User.FullName, Email = s.User.Email, Phone = s.User.Phone, Role = s.User.Role.ToString() } };
        }

        public async Task<StaffResponse> CreateStaffAsync(CreateStaffRequest request, int tenantId)
        {
            var staff = new Staff { UserId = request.UserId, Department = request.Department, Designation = request.Designation, Salary = request.Salary, JoiningDate = request.JoiningDate, BranchId = request.BranchId, TenantId = tenantId };
            _context.Staff.Add(staff);
            await _context.SaveChangesAsync();
            return (await GetStaffByIdAsync(staff.Id))!;
        }

        public async Task<StaffResponse> UpdateStaffAsync(int id, UpdateStaffRequest request)
        {
            var staff = await _context.Staff.FindAsync(id) ?? throw new KeyNotFoundException("Staff not found");
            if (request.Department != null) staff.Department = request.Department;
            if (request.Designation != null) staff.Designation = request.Designation;
            if (request.Salary.HasValue) staff.Salary = request.Salary;
            if (request.BranchId.HasValue) staff.BranchId = request.BranchId;
            await _context.SaveChangesAsync();
            return (await GetStaffByIdAsync(id))!;
        }

        public async Task<bool> DeleteStaffAsync(int id)
        {
            var staff = await _context.Staff.FindAsync(id) ?? throw new KeyNotFoundException("Staff not found");
            staff.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
