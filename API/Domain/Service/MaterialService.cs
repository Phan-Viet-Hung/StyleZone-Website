using API.Domain.DTOs;
using API.Domain.Extentions;
using API.Domain.Request.MaterialRequest;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.Service
{
    public class MaterialService : IMaterialService
    {
        private readonly DbContextApp _context;

        public MaterialService(DbContextApp context)
        {
            _context = context;
        }

        public async Task<List<MaterialDto>> GetAllAsync()
        {
            return await _context.Materials
                .Select(m => new MaterialDto
                {
                    Id = m.Id,
                    Name = m.Name ?? string.Empty,
                    Description = m.Description,
                    TotalQuantity = _context.ProductDetails
                        .Where(pd => pd.MaterialId == m.Id)
                        .Sum(pd => (int?)pd.Quantity) ?? 0,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync();
        }


        public async Task<MaterialDto?> GetByIdAsync(Guid id)
        {
            var material = await _context.Materials.FindAsync(id);
            return material?.ToDto();
        }

        public async Task<MaterialDto> CreateAsync(CreateMaterialRequest request)
        {
            if (await _context.Materials.AnyAsync(m => m.Name == request.Name))
                throw new Exception("Tên chất liệu đã tồn tại.");

            var material = new Material
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.Materials.Add(material);
            await _context.SaveChangesAsync();
            return material.ToDto();
        }

        public async Task<MaterialDto> UpdateAsync(UpdateMaterialRequest request)
        {
            var material = await _context.Materials.FindAsync(request.Id);
            if (material == null)
                throw new Exception("Không tìm thấy chất liệu.");

            if (await _context.Materials.AnyAsync(m => m.Name == request.Name && m.Id != request.Id))
                throw new Exception("Tên chất liệu đã tồn tại cho một bản ghi khác.");

            material.Name = request.Name.Trim();
            material.Description = request.Description?.Trim();
            material.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return material.ToDto();
        }

    }
}
