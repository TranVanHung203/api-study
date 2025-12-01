
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
            // Có thể login bằng username hoặc email
            var user = await _userRepo.GetByUsernameAsync(loginDto.UsernameOrEmail);
            if (user == null)
            {
                user = await _userRepo.GetByEmailAsync(loginDto.UsernameOrEmail);
            }

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Sai tài khoản/email hoặc mật khẩu");

            // Kiểm tra email đã xác thực chưa
            if (!user.EmailConfirmed)
                throw new UnauthorizedAccessException("Email chưa được xác thực. Vui lòng kiểm tra email để xác thực đăng ký.");

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

        public async Task RegisterAsync(RegisterDto registerDto)
        {
            // Validate uniqueness
            var byUsername = await _userRepo.GetByUsernameAsync(registerDto.Username);
            if (byUsername != null) throw new InvalidOperationException("Username đã tồn tại");

            var byEmail = await _userRepo.GetByEmailAsync(registerDto.Email);
            if (byEmail != null) throw new InvalidOperationException("Email đã tồn tại");

            // Generate verification code (6 digits)
            var verificationCode = Random.Shared.Next(100000, 999999).ToString();

            // Create user (not confirmed yet)
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FullName = registerDto.FullName ?? string.Empty,
                EmailConfirmed = false,
                EmailConfirmationToken = verificationCode,
                EmailConfirmationExpiry = DateTime.UtcNow.AddMinutes(1), // Code expires in 15 minutes
                IsGuest = false,
                ExpiresAt = null
            };

            await _userRepo.CreateAsync(user);

            // Send verification email
            var subject = "Xác thực đăng ký tài khoản";
            var body = $@"
<h2>Xác thực đăng ký</h2>
<p>Mã xác thực của bạn là: <strong>{verificationCode}</strong></p>
<p>Mã này sẽ hết hạn trong 15 phút.</p>
<p>Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>";

            await _emailSender.SendEmailAsync(registerDto.Email, subject, body);
        }

        public async Task VerifyEmailAsync(VerifyEmailDto verifyDto)
        {
            var user = await _userRepo.GetByEmailAsync(verifyDto.Email);
            if (user == null) throw new Exception("Email không tồn tại");

            if (user.EmailConfirmed)
                throw new InvalidOperationException("Email đã được xác thực rồi");

            if (user.EmailConfirmationToken != verifyDto.VerificationCode)
                throw new UnauthorizedAccessException("Mã xác thực không đúng");

            if (user.EmailConfirmationExpiry < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Mã xác thực đã hết hạn");

            // Verify email
            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationExpiry = null;
            await _userRepo.UpdateAsync(user);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto forgotDto)
        {
            var user = await _userRepo.GetByEmailAsync(forgotDto.Email);
            if (user == null) return; // Don't reveal if email exists

            // Generate verification code (6 digits)
            var verificationCode = Random.Shared.Next(100000, 999999).ToString();

            user.EmailConfirmationToken = verificationCode;
            user.EmailConfirmationExpiry = DateTime.UtcNow.AddMinutes(15); // Code expires in 15 minutes
            await _userRepo.UpdateAsync(user);

            // Send verification email
            var subject = "Mã xác thực đặt lại mật khẩu";
            var body = $@"
<h2>Đặt lại mật khẩu</h2>
<p>Mã xác thực của bạn là: <strong>{verificationCode}</strong></p>
<p>Mã này sẽ hết hạn trong 15 phút.</p>
<p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>";

            await _emailSender.SendEmailAsync(forgotDto.Email, subject, body);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto resetDto)
        {
            var user = await _userRepo.GetByEmailAsync(resetDto.Email);
            if (user == null) throw new Exception("Email không tồn tại");

            if (user.EmailConfirmationToken != resetDto.VerificationCode)
                throw new UnauthorizedAccessException("Mã xác thực không đúng");

            if (user.EmailConfirmationExpiry < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Mã xác thực đã hết hạn");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);
            user.EmailConfirmationToken = null;
            user.EmailConfirmationExpiry = null;
            await _userRepo.UpdateAsync(user);
        }

        public async Task ResendVerificationCodeAsync(ResendVerificationCodeDto resendDto)
        {
            var user = await _userRepo.GetByEmailAsync(resendDto.Email);
            if (user == null) throw new Exception("Email không tồn tại");

            // Generate new verification code (6 digits)
            var verificationCode = Random.Shared.Next(100000, 999999).ToString();

            user.EmailConfirmationToken = verificationCode;
            user.EmailConfirmationExpiry = DateTime.UtcNow.AddMinutes(15); // Code expires in 15 minutes
            await _userRepo.UpdateAsync(user);

            // Send email based on type
            if (resendDto.Type == "register")
            {
                var subject = "Xác thực đăng ký tài khoản";
                var body = $@"
<h2>Xác thực đăng ký</h2>
<p>Mã xác thực của bạn là: <strong>{verificationCode}</strong></p>
<p>Mã này sẽ hết hạn trong 15 phút.</p>
<p>Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>";

                await _emailSender.SendEmailAsync(resendDto.Email, subject, body);
            }
            else if (resendDto.Type == "forgot-password")
            {
                var subject = "Mã xác thực đặt lại mật khẩu";
                var body = $@"
<h2>Đặt lại mật khẩu</h2>
<p>Mã xác thực của bạn là: <strong>{verificationCode}</strong></p>
<p>Mã này sẽ hết hạn trong 15 phút.</p>
<p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>";

                await _emailSender.SendEmailAsync(resendDto.Email, subject, body);
            }
        }

        public async Task<AuthResponseDto> CreateGuestUserAsync()
        {
            // Generate unique username and email for guest
            var guestId = Guid.NewGuid().ToString("N").Substring(0, 12);
            var username = $"guest_{guestId}";
            var email = $"guest_{guestId}@example.com";
            var password = Guid.NewGuid().ToString(); // Random password

            // Create guest user
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FullName = $"Guest {guestId}",
                IsGuest = true,
                ExpiresAt = DateTime.UtcNow.AddHours(24) // Expire after 24 hours
            };

            var created = await _userRepo.CreateAsync(user);

            // Generate JWT and refresh token
            var jwt = GenerateJwtToken(created);
            var refresh = await _refreshRepo.CreateRefreshTokenAsync(created.Id);

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                RefreshToken = refresh.Token,
                Expiration = jwt.ValidTo
            };
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
                new Claim("fullname", user.FullName ?? ""),
                new Claim("isguest", user.IsGuest.ToString().ToLower())
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
