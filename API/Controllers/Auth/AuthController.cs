using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Services.Interfaces;

namespace BillingSoftware.API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) { _authService = authService; }

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
    }
}
