using API.Domain.DTOs;
using API.Domain.Request.OriginRequest;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.Service
{
    public class OriginService : IOriginService
    {
        private readonly DbContextApp _context;

        public OriginService(DbContextApp context)
        {
            _context = context;
        }

        public async Task<List<OriginDto>> GetAllAsync()
        {
            return await _context.Origins
                .Select(m => new OriginDto
                {
                    Id = m.Id,
                    Name = m.Name ?? string.Empty,
                    Description = m.Description,
                    TotalQuantity = _context.ProductDetails
                        .Where(pd => pd.OriginId == m.Id)
                        .Sum(pd => (int?)pd.Quantity) ?? 0,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<OriginDto?> GetByIdAsync(Guid id)
        {
            var x = await _context.Origins.FindAsync(id);
            if (x == null) return null;
            return new OriginDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            };
        }

        public async Task<OriginDto> CreateAsync(CreateOriginRequest request)
        {
            if (await _context.Origins.AnyAsync(o => o.Name == request.Name))
                throw new Exception("Tên xuất xứ đã tồn tại.");

            var origin = new Origin
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.Now
            };

            _context.Origins.Add(origin);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(origin.Id) ?? throw new Exception("Tạo thất bại.");
        }

        public async Task<OriginDto> UpdateAsync(UpdateOriginRequest request)
        {
            var origin = await _context.Origins.FindAsync(request.Id);
            if (origin == null)
                throw new Exception("Xuất xứ không tồn tại.");

            if (await _context.Origins.AnyAsync(o => o.Name == request.Name && o.Id != request.Id))
                throw new Exception("Tên xuất xứ đã tồn tại.");

            origin.Name = request.Name;
            origin.Description = request.Description;
            origin.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(origin.Id) ?? throw new Exception("Cập nhật thất bại.");
        }
    }
}
