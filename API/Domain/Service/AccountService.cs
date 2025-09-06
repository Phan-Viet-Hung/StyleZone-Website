using API.Configuration;
using API.Domain.DTOs;
using API.Domain.Request.AccountRequest;
using API.Domain.Service.IService;
using API.Request;
using DAL_Empty.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace API.Service
{
    public class AccountService : IAccountService
    {
        private readonly DbContextApp _context;
        private readonly Random _random = new();
        private readonly SmtpSettings _smtpSettings;

        public AccountService(DbContextApp context, IOptions<SmtpSettings> smtpOptions)
        {
            _context = context;
            _smtpSettings = smtpOptions.Value;
        }

        public async Task<List<AccountWithPasswordDto>> CreateAccountsAsync(List<CreateAccountRequest> requests)
        {
            return await CreateValidatedAccountsAsync(requests);
        }

        public async Task<ImportResult> ImportAccountsFromExcelAsync(IFormFile file)
        {
            var result = new ImportResult();
            var requests = new List<CreateAccountRequest>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.First();
            int rowCount = worksheet.Dimension.Rows;

            var existingEmails = _context.Accounts.Select(a => a.Email.ToLower()).ToHashSet();
            var existingPhones = _context.Accounts.Select(a => a.PhoneNumber).ToHashSet();

            var roles = await _context.Roles.ToDictionaryAsync(r => r.Name.Trim().ToLower(), r => r.Id);

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var email = worksheet.Cells[row, 3].Text?.Trim().ToLower();
                    var phone = worksheet.Cells[row, 4].Text?.Trim();

                    if (existingEmails.Contains(email) || existingPhones.Contains(phone))
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    var roleName = worksheet.Cells[row, 7].Text?.Trim().ToLower();
                    Guid? roleId = null;
                    if (!string.IsNullOrWhiteSpace(roleName) && roles.ContainsKey(roleName))
                        roleId = roles[roleName];

                    var request = new CreateAccountRequest
                    {
                        Name = worksheet.Cells[row, 1].Text,
                        Birthday = DateTime.Parse(worksheet.Cells[row, 2].Text),
                        Email = email,
                        PhoneNumber = phone,
                        Gender = int.TryParse(worksheet.Cells[row, 5].Text, out var genderInt) ? (GenderEnum)genderInt : GenderEnum.Nam,
                        Address = worksheet.Cells[row, 6].Text,
                        RoleId = roleId,
                        IsActive = true
                    };

                    if (string.IsNullOrWhiteSpace(request.UserName))
                        request.UserName = request.Email;
                    if (string.IsNullOrWhiteSpace(request.Password))
                        request.Password = GenerateSecurePassword();

                    var context = new ValidationContext(request);
                    Validator.ValidateObject(request, context, true);

                    requests.Add(request);
                    existingEmails.Add(email);
                    existingPhones.Add(phone);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Dòng {row}: {ex.Message}");
                }
            }


            result.CreatedAccounts = await CreateValidatedAccountsAsync(requests);
            result.SuccessCount = result.CreatedAccounts.Count;
            return result;
        }

        private async Task<List<AccountWithPasswordDto>> CreateValidatedAccountsAsync(List<CreateAccountRequest> requests)
        {
            var result = new List<AccountWithPasswordDto>();

            foreach (var request in requests)
            {
                var role = await _context.Roles.FindAsync(request.RoleId);
                if (request.RoleId == null || role == null)
                    throw new InvalidOperationException($"Không tìm thấy chức vụ với RoleId: {request.RoleId}");

                var birthday = request.Birthday.Date;
                var today = DateTime.Today;
                var age = today.Year - birthday.Year;
                if (birthday > today.AddYears(-age)) age--;
                if (age < 18)
                    throw new InvalidOperationException($"Người dùng {request.Name} phải đủ 18 tuổi trở lên.");

                if (await _context.Accounts.AnyAsync(a => a.Email == request.Email.Trim()))
                    throw new InvalidOperationException($"Email {request.Email} đã tồn tại");

                if (await _context.Accounts.AnyAsync(a => a.PhoneNumber == request.PhoneNumber.Trim()))
                    throw new InvalidOperationException($"SĐT {request.PhoneNumber} đã tồn tại");

                var userName = request.Email?.Trim();
                if (!string.IsNullOrEmpty(userName) && userName.Contains("@"))
                {
                    userName = userName.Split('@')[0];
                }

                var rawPassword = string.IsNullOrWhiteSpace(request.Password) ? GenerateSecurePassword() : request.Password.Trim();

                var account = new Account
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name?.Trim(),
                    Birthday = birthday,
                    Email = request.Email?.Trim(),
                    PhoneNumber = request.PhoneNumber?.Trim(),
                    UserName = userName,
                    Gender = request.Gender,
                    Password = BCrypt.Net.BCrypt.HashPassword(rawPassword),
                    Address = request.Address?.Trim(),
                    RoleId = request.RoleId,
                    IsActive = true
                };

                _context.Accounts.Add(account);

                var dto = new AccountWithPasswordDto
                {
                    Id = account.Id,
                    Name = account.Name,
                    Birthday = account.Birthday,
                    Email = account.Email,
                    PhoneNumber = account.PhoneNumber,
                    UserName = account.UserName,
                    Gender = account.Gender,
                    Address = account.Address,
                    roleName = role.Name,
                    IsActive = account.IsActive,
                    RawPassword = rawPassword
                };

                result.Add(dto);
                await SendAccountCreatedEmailAsync(dto);
            }

            await _context.SaveChangesAsync();
            return result;
        }

        private string GenerateSecurePassword()
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "@$!%*?&";

            string password =
                upper[_random.Next(upper.Length)].ToString() +
                lower[_random.Next(lower.Length)].ToString() +
                digits[_random.Next(digits.Length)].ToString() +
                special[_random.Next(special.Length)].ToString();

            string all = upper + lower + digits + special;
            password += new string(Enumerable.Repeat(all, 4).Select(s => s[_random.Next(s.Length)]).ToArray());

            return new string(password.OrderBy(c => _random.Next()).ToArray());
        }

        private async Task SendAccountCreatedEmailAsync(AccountWithPasswordDto account)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_smtpSettings.User));
            message.To.Add(MailboxAddress.Parse(account.Email));
            message.Subject = "Thông tin tài khoản StyleZone";

            message.Body = new TextPart("html")
            {
                Text = $@"
                    <p>Xin chào <b>{account.Name}</b>,</p>
                    <p>Tài khoản của bạn đã được tạo thành công tại <b>StyleZone</b>:</p>
                    <ul>
                        <li><b>Tên đăng nhập:</b> {account.UserName}</li>
                        <li><b>Mật khẩu:</b> {account.RawPassword}</li>
                    </ul>
                    <p>Vui lòng đăng nhập và đổi mật khẩu ngay sau lần đăng nhập đầu tiên để đảm bảo bảo mật.</p>
                    <p>Trân trọng,<br/>StyleZone Team</p>"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpSettings.Smtp, _smtpSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpSettings.User, _smtpSettings.Pass);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }

        public async Task<List<AccountDto>> GetAllAccountsAsync()
        {
            return await _context.Accounts
                .Include(a => a.Role)
                .Select(a => new AccountDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Birthday = a.Birthday,
                    Email = a.Email,
                    PhoneNumber = a.PhoneNumber,
                    UserName = a.UserName,
                    Gender = a.Gender,
                    Address = a.Address,
                    roleName = a.Role.Name,
                    IsActive = a.IsActive
                })
                .ToListAsync();
        }

        public async Task<AccountDto> GetByIdAsync(Guid id)
        {
            var account = await _context.Accounts.Include(a => a.Role).FirstOrDefaultAsync(a => a.Id == id);
            if (account == null) return null;

            return new AccountDto
            {
                Id = account.Id,
                Name = account.Name,
                Birthday = account.Birthday,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                UserName = account.UserName,
                Gender = account.Gender,
                Address = account.Address,
                roleName = account.Role?.Name,
                IsActive = account.IsActive
            };
        }

        public async Task<AccountDto> GetByPhoneNumberAsync(string phoneNumber)
        {
            var account = await _context.Accounts.Include(a => a.Role).FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber);
            if (account == null) return null;

            return new AccountDto
            {
                Id = account.Id,
                Name = account.Name,
                Birthday = account.Birthday,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                UserName = account.UserName,
                Gender = account.Gender,
                Address = account.Address,
                roleName = account.Role?.Name,
                IsActive = account.IsActive
            };
        }

        public async Task<AccountDto> UpdateAccountAsync(Guid id, UpdateAccountRequest request)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) throw new KeyNotFoundException("Không tìm thấy tài khoản");

            var birthday = request.Birthday?.Date;
            if (birthday.HasValue)
            {
                var today = DateTime.Today;
                var age = today.Year - birthday.Value.Year;
                if (birthday > today.AddYears(-age)) age--;
                if (age < 18)
                    throw new InvalidOperationException("Người dùng phải đủ 18 tuổi trở lên.");
                account.Birthday = birthday.Value;
            }

            if (await _context.Accounts.AnyAsync(a => a.Email == request.Email && a.Id != id))
                throw new InvalidOperationException("Email đã tồn tại");

            if (await _context.Accounts.AnyAsync(a => a.UserName == request.UserName && a.Id != id))
                throw new InvalidOperationException("Username đã tồn tại");

            if (await _context.Accounts.AnyAsync(a => a.PhoneNumber == request.PhoneNumber && a.Id != id))
                throw new InvalidOperationException("Số điện thoại đã tồn tại");

            if (request.RoleId.HasValue)
            {
                var role = await _context.Roles.FindAsync(request.RoleId.Value);
                if (role == null)
                    throw new InvalidOperationException("Không tìm thấy chức vụ");
                account.RoleId = request.RoleId.Value;
            }

            account.Name = request.Name?.Trim() ?? account.Name;
            account.Email = request.Email?.Trim() ?? account.Email;
            account.PhoneNumber = request.PhoneNumber?.Trim() ?? account.PhoneNumber;
            account.UserName = request.UserName?.Trim() ?? account.UserName;
            account.Gender = request.Gender ?? account.Gender;
            account.Address = request.Address?.Trim() ?? account.Address;
            account.IsActive = request.IsActive;
            if (!string.IsNullOrEmpty(request.Password))
                account.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _context.SaveChangesAsync();

            var roleName = await _context.Roles.Where(r => r.Id == account.RoleId).Select(r => r.Name).FirstOrDefaultAsync();

            return new AccountDto
            {
                Id = account.Id,
                Name = account.Name,
                Birthday = account.Birthday,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                UserName = account.UserName,
                Gender = account.Gender,
                Address = account.Address,
                roleName = roleName,
                IsActive = account.IsActive
            };
        }

        public async Task<bool> ToggleActiveStatusAsync(Guid id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return false;

            account.IsActive = !account.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetActiveStatusesAsync(List<SetActiveStatusRequest> requests)
        {
            foreach (var req in requests)
            {
                var account = await _context.Accounts.FindAsync(req.Id);
                if (account != null)
                    account.IsActive = req.IsActive;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateProfileAsync(Guid id, UpdateProfileRequest request)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return false;

            account.PhoneNumber = request.PhoneNumber;
            account.Address = request.Address;
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, account.Password))
                    return false;
                if (request.NewPassword != request.ConfirmPassword)
                    return false;
                account.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            }

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsUserInRoleAsync(Guid userId, string roleName)
        {
            return await _context.Accounts
                .Include(a => a.Role)
                .AnyAsync(a => a.Id == userId && a.Role != null && a.Role.Name == roleName);
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Select(a => new RoleDto { Id = a.Id, Name = a.Name })
                .ToListAsync();
        }
    }
}
