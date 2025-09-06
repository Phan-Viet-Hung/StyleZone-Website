using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;
using API.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using MailKit.Net.Smtp;
using System.Security.Claims;
using System.Text;
using API.Domain.Request.AccountRequest;

namespace API.Service
{
    public class AuthService : IAuthService
    {
        private readonly DbContextApp _context;
        private readonly IConfiguration _config;

        public AuthService(DbContextApp context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        #region Login
        public async Task<string> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không được để trống.");

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.UserName == username && a.IsActive);

            if (account == null)
                throw new UnauthorizedAccessException("Tài khoản không tồn tại hoặc đã bị khóa.");

            bool passwordMatch = BCrypt.Net.BCrypt.Verify(password, account.Password);
            if (!passwordMatch)
                throw new UnauthorizedAccessException("Mật khẩu không đúng.");

            // Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.UserName ?? string.Empty),
                new Claim(ClaimTypes.Role, account.Role?.Name ?? "User")
            };

            // Key và Expire
            var secretKey = _config["JwtSettings:SecretKey"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            int expireMinutes = int.TryParse(_config["JwtSettings:ExpireMinutes"], out var minutes) ? minutes : 60;

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion

        #region Forgot Password
        public async Task<ForgotPasswordResponse> ForgotPasswordAsync(string emailOrUsername)
        {
            // 1. Lấy cấu hình SMTP
            var smtpConfig = _config.GetSection("Mail").Get<SmtpSettings>();
            if (smtpConfig == null)
                throw new InvalidOperationException("Không tìm thấy cấu hình SMTP trong appsettings.json.");

            var smtpPassword = smtpConfig.Pass;

            // 2. Chuẩn hóa input
            var emailOrUsernameNormalized = (emailOrUsername ?? string.Empty).Trim().ToLower();

            // 3. Tìm tài khoản
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    (a.Email != null && a.Email.ToLower() == emailOrUsernameNormalized)
                );

            if (account == null)
                throw new KeyNotFoundException("Không tìm thấy tài khoản.");

            if (string.IsNullOrWhiteSpace(account.Email))
                throw new InvalidOperationException($"Tài khoản {account.UserName} không có email để gửi code.");

            // 4. Sinh mã code (6 chữ số)
            var rnd = new Random();
            var code = rnd.Next(100000, 999999).ToString();

            // 5. Tạo resetToken (JWT) chứa email + code, exp 10 phút
            var resetToken = GenerateResetTokenWithCode(account.Email, code, minutesValid: 10);

            // 6. Soạn email
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(smtpConfig.User));
            message.To.Add(MailboxAddress.Parse(account.Email));
            message.Subject = "Mã đặt lại mật khẩu - StyleZone";
            message.Body = new TextPart("plain")
            {
                Text = $"Xin chào {account.Name ?? account.UserName},\n\n" +
                       $"Mã đặt lại mật khẩu của bạn là: {code}\n" +
                       $"Mã có hiệu lực trong 10 phút.\n\n" +
                       $"Nếu bạn không yêu cầu, vui lòng bỏ qua email này.\n\n" +
                       $"Trân trọng,\nStyleZone Team"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpConfig.Smtp, smtpConfig.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpConfig.User, smtpPassword);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            return new ForgotPasswordResponse
            {
                Code = code,           // CHỈ DÙNG CHO TEST
                ResetToken = resetToken,
                Message = "Mã đã gửi tới email (kiểm tra spam)."
            };
        }
        #endregion

        #region Reset Password
        public async Task<bool> ResetPasswordWithCodeAsync(string token, string code, string newPassword, string confirmPassword)
        {
            var claims = ValidateResetTokenWithCode(token);
            if (claims == null)
                return false;

            var email = claims.Value.Email;
            var tokenCode = claims.Value.Code;

            if (string.IsNullOrWhiteSpace(tokenCode) || tokenCode != code)
                return false;

            // ✅ Kiểm tra mật khẩu xác nhận
            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
                return false;

            if (newPassword != confirmPassword)
                return false;

            // ✅ Kiểm tra độ dài mật khẩu mới (ví dụ >= 6 ký tự)
            if (newPassword.Length < 6)
                return false;

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
            if (account == null)
                return false;

            // ✅ Hash và lưu mật khẩu mới
            account.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Helpers for token-with-code
        private string GenerateResetTokenWithCode(string email, string code, int minutesValid = 10)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim("reset_code", code)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutesValid),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private (string Email, string Code)? ValidateResetTokenWithCode(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var email = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
                var code = jwtToken.Claims.FirstOrDefault(x => x.Type == "reset_code")?.Value;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
                    return null;

                return (Email: email, Code: code);
            }
            catch
            {
                return null;
            }
        }
        #endregion
    }

    
}
