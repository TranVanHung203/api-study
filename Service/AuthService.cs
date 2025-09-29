
using Entities.DTOs;
using Entities.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Service.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System;
using BCrypt.Net;
using Contracts;
namespace Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshRepo;
        private readonly IEmailSenderService _emailSender;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepo, IRefreshTokenRepository refreshRepo,
                           IEmailSenderService emailSender, IConfiguration config)
        {
            _userRepo = userRepo;
            _refreshRepo = refreshRepo;
            _emailSender = emailSender;
            _config = config;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepo.GetByUsernameAsync(loginDto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Sai tài khoản hoặc mật khẩu");

            var jwt = GenerateJwtToken(user);
            var refresh = await _refreshRepo.CreateRefreshTokenAsync(user.Id);

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                RefreshToken = refresh.Token,
                Expiration = jwt.ValidTo
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = await _refreshRepo.GetByTokenAsync(refreshToken);
            if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.Expires < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token không hợp lệ");

            var user = await _userRepo.GetByIdAsync(tokenEntity.UserId);
            if (user == null) throw new UnauthorizedAccessException("User không tồn tại");

            var jwt = GenerateJwtToken(user);
            var newRefresh = await _refreshRepo.RotateRefreshTokenAsync(tokenEntity);

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                RefreshToken = newRefresh.Token,
                Expiration = jwt.ValidTo
            };
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto forgotDto)
        {
            var user = await _userRepo.GetByEmailAsync(forgotDto.Email);
            if (user == null) return;

            user.EmailConfirmationToken = Guid.NewGuid().ToString();
            user.EmailConfirmationExpiry = DateTime.UtcNow.AddHours(1);
            await _userRepo.UpdateAsync(user);

            var link = $"http://192.168.1.140:8080/reset-password?email={user.Email}&token={Uri.EscapeDataString(user.EmailConfirmationToken)}";
            await _emailSender.SendEmailAsync(user.Email, "Reset mật khẩu", link);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto resetDto)
        {
            var user = await _userRepo.GetByEmailAsync(resetDto.Email);
            if (user == null) throw new Exception("User không tồn tại");

            if (user.EmailConfirmationToken != resetDto.Token || user.EmailConfirmationExpiry < DateTime.UtcNow)
                throw new Exception("Token không hợp lệ hoặc đã hết hạn");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);
            user.EmailConfirmationToken = null;
            user.EmailConfirmationExpiry = null;
            await _userRepo.UpdateAsync(user);
        }

        private JwtSecurityToken GenerateJwtToken(User user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var keyString = jwtSettings["Key"] ?? throw new InvalidOperationException("JwtSettings:Key is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim("fullname", user.FullName ?? "")
                ,new Claim(ClaimTypes.Role, user.Role ?? "Assistant")
            };

            // Token lifetime: read from config `JwtSettings:ExpiresInMinutes` or default to 60
            int expiresInMinutes = 60;
            if (int.TryParse(jwtSettings["ExpiresInMinutes"], out var parsed))
                expiresInMinutes = parsed;

            return new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
        }
    }
}
