using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MasterBilling.API.DTOs.Common;
using MasterBilling.API.DTOs.Requests;
using MasterBilling.API.DTOs.Responses;
using MasterBilling.API.Services.Interfaces;

namespace MasterBilling.API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
        { var result = await _authService.RegisterAsync(request); return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Registration successful")); }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
        { var result = await _authService.LoginAsync(request); return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful")); }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
        { var result = await _authService.RefreshTokenAsync(request); return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Token refreshed")); }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        { var result = await _authService.ForgotPasswordAsync(request); return Ok(ApiResponse<bool>.SuccessResponse(result, "Password reset instructions sent")); }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordRequest request)
        { var result = await _authService.ResetPasswordAsync(request); return Ok(ApiResponse<bool>.SuccessResponse(result, "Password reset successful")); }

        [HttpPost("verify-otp")]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyOtp([FromBody] VerifyOtpRequest request)
        { var result = await _authService.VerifyOtpAsync(request); return Ok(ApiResponse<bool>.SuccessResponse(result, "OTP verified")); }

        /// <summary>Returns the profile of the currently authenticated user (JWT required).</summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetMe()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value
                         ?? User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return Unauthorized(ApiResponse<UserResponse>.FailResponse("Unable to identify user from token."));
            var user = await _userService.GetUserByIdAsync(userId);
            return user == null
                ? NotFound(ApiResponse<UserResponse>.FailResponse("User not found"))
                : Ok(ApiResponse<UserResponse>.SuccessResponse(user));
        }
    }
}
