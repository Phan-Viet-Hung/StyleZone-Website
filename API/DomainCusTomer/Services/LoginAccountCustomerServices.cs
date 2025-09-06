using API.Domain.DTOs;
using API.DomainCusTomer.Config;
using API.DomainCusTomer.DTOs.AccountCustomer;
using API.DomainCusTomer.ExTentions;
using API.DomainCusTomer.Request.AccountCustomerRequest;
using API.DomainCusTomer.Request.LoginAccountCustomerRequest;
using API.DomainCusTomer.Services.IServices;
using DAL_Empty.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.EntityFrameworkCore;

namespace API.DomainCusTomer.Services
{
    public class LoginAccountCustomerServices : ILoginAccountCustomerServices
    {
        private readonly DbContextApp _context;
        private readonly JwtTokenHelper _jwtHelper;

        public LoginAccountCustomerServices(DbContextApp context, JwtTokenHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }
        private static string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public bool CheckEmail(string email)
        {
            var check = _context.Customers.FirstOrDefault(propa => propa.Email == email);
            if (check == null)
            {
                return true;
            }
            return false;
        }



        public async Task<Customer> forgotpassword(ForgotpasswordCustomerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Không được để trống.");

            var user = await _context.Customers.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (user == null)
                throw new ArgumentException("Email không tồn tại.");

            user.Password = request.NewPassword.HashPassword();
            user.UpdateAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return user;
        }
        public async Task<Customer> LoginGoole(LoginGoogleCustomerRequest request)
        {
            var emailExists = await _context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email);

            if (emailExists != null)
            {
                // 👉 Chỉ check khi đã tồn tại
                if (emailExists.Status != "Active")
                    throw new UnauthorizedAccessException("Tài khoản của bạn đã bị khóa.");

                // Kiểm tra giỏ hàng
                var existingCart = await _context.Carts.FirstOrDefaultAsync(c => c.CustomerId == emailExists.Id);
                if (existingCart == null)
                {
                    _context.Carts.Add(new Cart
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = emailExists.Id,
                        CreateAt = DateTime.Now,
                    });
                    await _context.SaveChangesAsync();
                }

                return emailExists;
            }

            // 👉 Nếu chưa có thì tạo mới (Active mặc định)
            var account = new Customer
            {
                Id = Guid.NewGuid(),
                Fullname = request.Name,
                Email = request.Email,
                UserName = request.Email.Split('@')[0],
                Password = GenerateRandomPassword(12).HashPassword(),
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,
                Status = "Active",
            };

            _context.Customers.Add(account);

            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = account.Id,
                CreateAt = DateTime.Now,
            };
            _context.Carts.Add(cart);

            await _context.SaveChangesAsync();
            return account;
        }
        public async Task<Customer?> LoginAsync(LoginnCustomerRequest request)
        {
            var user = await _context.Customers
                .FirstOrDefaultAsync(x => x.UserName == request.UserName);

            if (user == null || !request.Password.VerifyPassword(user.Password!))
                throw new UnauthorizedAccessException("Sai tài khoản hoặc mật khẩu mời nhập lại.");

            // Check trạng thái
            if (user.Status != "Active")
                throw new UnauthorizedAccessException("Tài khoản của bạn bị khóa.");

            return user;
        }
        public async Task<RegisterCustomerDto> RegisterAsync(RegisteCustomerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.PhoneNumber))
                throw new ArgumentException("Không được để trống.");

            var account = new Customer
            {
                Id = Guid.NewGuid(),
                Fullname = request.Name,
                Email = request.Email,
                UserName = request.Email.Split('@')[0],
                Password = request.Password.HashPassword(),
                PhoneNumber = request.PhoneNumber,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,
                Status = "Active"
            };

            _context.Customers.Add(account);
            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = account.Id,
                CreateAt = DateTime.Now,
            };
            _context.Carts.Add(cart);

            await _context.SaveChangesAsync();
            return account.ToRegisterDto();
        }

        public async Task GetByUsernameAsync(string username)
        {
            var account = await _context.Customers
                                        .FirstOrDefaultAsync(c => c.UserName == username);

            if (account == null)
                throw new KeyNotFoundException("Tài khoản không tồn tại");

            if (account.Status != "Active")
                throw new UnauthorizedAccessException("Tài khoản đã bị khóa hoặc không hoạt động");
        }

    }
}
