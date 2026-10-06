using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Services.Interfaces;
using System.Security.Claims;

namespace BillingSoftware.API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        public UserController(IUserService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetAll() => Ok(ApiResponse<List<UserResponse>>.SuccessResponse(await _service.GetAllUsersAsync(TenantId)));

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetById(int id) { var r = await _service.GetUserByIdAsync(id); return r == null ? NotFound(ApiResponse<UserResponse>.FailResponse("User not found")) : Ok(ApiResponse<UserResponse>.SuccessResponse(r)); }

        [HttpPost]
        [Authorize(Roles = "Owner,Manager,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> Create([FromBody] CreateUserRequest request) => Ok(ApiResponse<UserResponse>.SuccessResponse(await _service.CreateUserAsync(request, TenantId), "User created"));

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> Update(int id, [FromBody] UpdateUserRequest request) => Ok(ApiResponse<UserResponse>.SuccessResponse(await _service.UpdateUserAsync(id, request), "User updated"));

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _service.DeleteUserAsync(id), "User deleted"));

        [HttpPut("{id}/role")]
        [Authorize(Roles = "Owner,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateRole(int id, [FromBody] UpdateUserRoleRequest request) => Ok(ApiResponse<UserResponse>.SuccessResponse(await _service.UpdateUserRoleAsync(id, request), "Role updated"));
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Owner,SuperAdmin")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _service;
        public RoleController(IRoleService service) { _service = service; }

        [HttpGet] public async Task<ActionResult<ApiResponse<List<RoleResponse>>>> GetAll() => Ok(ApiResponse<List<RoleResponse>>.SuccessResponse(await _service.GetAllRolesAsync()));
        [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<RoleResponse>>> GetById(int id) { var r = await _service.GetRoleByIdAsync(id); return r == null ? NotFound(ApiResponse<RoleResponse>.FailResponse("Not found")) : Ok(ApiResponse<RoleResponse>.SuccessResponse(r)); }
        [HttpPost] public async Task<ActionResult<ApiResponse<RoleResponse>>> Create([FromBody] CreateRoleRequest request) => Ok(ApiResponse<RoleResponse>.SuccessResponse(await _service.CreateRoleAsync(request), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<RoleResponse>>> Update(int id, [FromBody] UpdateRoleRequest request) => Ok(ApiResponse<RoleResponse>.SuccessResponse(await _service.UpdateRoleAsync(id, request), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _service.DeleteRoleAsync(id), "Deleted"));
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Owner,SuperAdmin")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _service;
        public PermissionController(IPermissionService service) { _service = service; }

        [HttpGet] public async Task<ActionResult<ApiResponse<List<PermissionResponse>>>> GetAll() => Ok(ApiResponse<List<PermissionResponse>>.SuccessResponse(await _service.GetAllPermissionsAsync()));
        [HttpPost] public async Task<ActionResult<ApiResponse<PermissionResponse>>> Create([FromBody] CreatePermissionRequest request) => Ok(ApiResponse<PermissionResponse>.SuccessResponse(await _service.CreatePermissionAsync(request), "Created"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _service.DeletePermissionAsync(id), "Deleted"));
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _service;
        public StaffController(IStaffService service) { _service = service; }
        private int TenantId => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        [HttpGet] public async Task<ActionResult<ApiResponse<List<StaffResponse>>>> GetAll() => Ok(ApiResponse<List<StaffResponse>>.SuccessResponse(await _service.GetAllStaffAsync(TenantId)));
        [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<StaffResponse>>> GetById(int id) { var r = await _service.GetStaffByIdAsync(id); return r == null ? NotFound(ApiResponse<StaffResponse>.FailResponse("Not found")) : Ok(ApiResponse<StaffResponse>.SuccessResponse(r)); }
        [HttpPost] public async Task<ActionResult<ApiResponse<StaffResponse>>> Create([FromBody] CreateStaffRequest request) => Ok(ApiResponse<StaffResponse>.SuccessResponse(await _service.CreateStaffAsync(request, TenantId), "Created"));
        [HttpPut("{id}")] public async Task<ActionResult<ApiResponse<StaffResponse>>> Update(int id, [FromBody] UpdateStaffRequest request) => Ok(ApiResponse<StaffResponse>.SuccessResponse(await _service.UpdateStaffAsync(id, request), "Updated"));
        [HttpDelete("{id}")] public async Task<ActionResult<ApiResponse<bool>>> Delete(int id) => Ok(ApiResponse<bool>.SuccessResponse(await _service.DeleteStaffAsync(id), "Deleted"));
    }
}
