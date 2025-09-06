using API.Domain.DTOs;
using API.Domain.Request.CustomerRequest;
using API.Domain.Service.IService.ICustomerService;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.Service
{
    public class CustomerService:ICustomerService
    {
        private readonly DbContextApp _context;

        public CustomerService(DbContextApp context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            return await _context.Customers
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Fullname = c.Fullname,
                    Birthday = c.Birthday,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    UserName = c.UserName,
                    Gender = c.Gender,
                    Status = c.Status,
                    CreateAt = c.CreateAt,
                    UpdateAt = c.UpdateAt
                })
                .ToListAsync();
        }

        public async Task<CustomerDto?> GetByIdAsync(Guid id)
        {
            return await _context.Customers
                .Where(c => c.Id == id)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Fullname = c.Fullname,
                    Birthday = c.Birthday,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    UserName = c.UserName,
                    Gender = c.Gender,
                    Status = c.Status,
                    CreateAt = c.CreateAt,
                    UpdateAt = c.UpdateAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateStatusAsync(Guid id, string status)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;

            if (string.IsNullOrWhiteSpace(status) ||
                !(status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                  status.Equals("Disable", StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Trạng thái không hợp lệ. Chỉ chấp nhận 'Active' hoặc 'Disable'.");
            }

            customer.Status = status;
            customer.UpdateAt = DateTime.UtcNow;

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> UpdateStatusBulkAsync(List<Guid> ids, string status)
        {
            if (ids == null || !ids.Any())
                throw new ArgumentException("Danh sách ID không hợp lệ.");

            if (string.IsNullOrWhiteSpace(status) ||
                !(status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                  status.Equals("Disable", StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Trạng thái không hợp lệ. Chỉ chấp nhận 'Active' hoặc 'Disable'.");
            }

            var customers = await _context.Customers
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            if (!customers.Any())
                throw new InvalidOperationException("Không tìm thấy khách hàng nào.");

            foreach (var customer in customers)
            {
                customer.Status = status;
                customer.UpdateAt = DateTime.UtcNow;
            }

            _context.Customers.UpdateRange(customers);
            return await _context.SaveChangesAsync();
        }
    }
}
