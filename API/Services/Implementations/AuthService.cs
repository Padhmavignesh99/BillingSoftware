using BillingSoftware.API.Data;
using BillingSoftware.API.DTOs.Common;
using BillingSoftware.API.DTOs.Requests;
using BillingSoftware.API.DTOs.Responses;
using BillingSoftware.API.Models.Entities;
using BillingSoftware.API.Models.Enums;
using BillingSoftware.API.Configuration;
using BillingSoftware.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BillingSoftware.API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthService(ApplicationDbContext context, Microsoft.Extensions.Options.IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new ArgumentException("Email already registered");

            var business = new Business
            {
                Name = request.BusinessName ?? request.FullName + "'s Business",
                BusinessTypeId = request.BusinessTypeId,
                Email = request.Email
            };
            _context.Businesses.Add(business);
            await _context.SaveChangesAsync();

            // Create default settings
            _context.BusinessSettings.Add(new BusinessSettings { TenantId = business.Id });
            _context.InvoiceSettings.Add(new InvoiceSettings { TenantId = business.Id });

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Owner,
                TenantId = business.Id
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return GenerateAuthResponse(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password");

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return GenerateAuthResponse(user);
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var principal = GetPrincipalFromExpiredToken(request.Token);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid refresh token");

            return GenerateAuthResponse(user);
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return true; // Don't reveal if email exists

            user.ResetToken = Guid.NewGuid().ToString("N")[..8].ToUpper();
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            user.OTP = new Random().Next(100000, 999999).ToString();
            user.OTPExpiry = DateTime.UtcNow.AddMinutes(10);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.ResetToken == request.Token && u.ResetTokenExpiry > DateTime.UtcNow);
            if (user == null) throw new ArgumentException("Invalid or expired reset token");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifyOtpAsync(VerifyOtpRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.OTP == request.OTP && u.OTPExpiry > DateTime.UtcNow);
            if (user == null) throw new ArgumentException("Invalid or expired OTP");

            user.OTP = null;
            user.OTPExpiry = null;
            await _context.SaveChangesAsync();
            return true;
        }

        private AuthResponse GenerateAuthResponse(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("TenantId", user.TenantId?.ToString() ?? "0")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            var refreshToken = Guid.NewGuid().ToString("N");
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            _context.SaveChanges();

            return new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                Expiration = expiration,
                User = new UserResponse
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role.ToString(),
                    TenantId = user.TenantId,
                    ProfileImageUrl = user.ProfileImageUrl,
                    LastLoginAt = user.LastLoginAt,
                    IsActive = user.IsActive
                }
            };
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return principal;
        }
    }
}
