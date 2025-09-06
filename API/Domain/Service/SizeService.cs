using API.Domain.DTOs;
using API.Domain.Extentions;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.Service
{
    public class SizeService : ISizeService
    {
        private readonly DbContextApp _context;

        public SizeService(DbContextApp context)
        {
            _context = context;
        }

        public async Task<List<SizeDto>> GetAllAsync()
        {
            var sizes = await _context.Sizes.ToListAsync();
            return sizes.OrderBy(p=>p.Name).Select(s => s.ToDto()).ToList();
        }

        public async Task<SizeDto?> GetByIdAsync(Guid id)
        {
            var size = await _context.Sizes.FindAsync(id);
            return size?.ToDto();
        }

        public async Task<SizeDto> CreateAsync(string code, string name)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                throw new Exception("Code và Name không được để trống.");

            if (await _context.Sizes.AnyAsync(s => s.Code == code))
                throw new Exception("Mã size đã tồn tại.");

            var size = new Size
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                CreatedAt = DateTime.Now
            };

            _context.Sizes.Add(size);
            await _context.SaveChangesAsync();
            return size.ToDto();
        }

        public async Task<SizeDto> UpdateAsync(Guid id, string code, string name)
        {
            var size = await _context.Sizes.FindAsync(id);
            if (size == null)
                throw new Exception("Không tìm thấy size.");

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                throw new Exception("Code và Name không được để trống.");

            if (await _context.Sizes.AnyAsync(s => s.Code == code && s.Id != id))
                throw new Exception("Mã size đã tồn tại cho một size khác.");

            size.Code = code;
            size.Name = name;
            size.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return size.ToDto();
        }

    }
}
