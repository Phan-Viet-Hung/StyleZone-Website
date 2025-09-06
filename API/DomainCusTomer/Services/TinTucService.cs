using API.DomainCusTomer.DTOs.Tintuc;
using API.DomainCusTomer.Services.IServices;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.DomainCusTomer.Services
{
    public class TinTucService : ITinTucService
    {
        private readonly DbContextApp _context;
        public TinTucService(DbContextApp context)
        {
            _context = context;
        }
        public async Task<List<TinTucDto>> GetAllTinTucAsync()
        {
            var news = await _context.Promotions
                .Where(x => x.Status == VoucherStatus.Active
                         && x.StartDate <= DateTime.Now
                        && x.EndDate >= DateTime.Now)
                .OrderByDescending(x => x.StartDate)
                .Select(x => new TinTucDto
                {
                    Id = x.Id,
                    Title = x.Name,
                    ImageUrl = x.ImageUrl, // Giả sử Image là URL đầy đủ hoặc tên file
                    ShortDescription = x.Description != null && x.Description.Length > 150
                        ? x.Description.Substring(0, 150) + "..."
                        : x.Description,
                    CreatedDate = x.StartDate.Value
                })
                .ToListAsync();
            return news;
        }


        public async Task<TinTucDetailDto> GetTinTucByIdAsync(Guid id)
        {
            var newsDetail = await _context.Promotions
                .Where(x => x.Id == id)
                .Select(x => new TinTucDetailDto
                {
                    Id = x.Id,
                    Title = x.Name,
                    ImageUrl = x.ImageUrl,
                    Content = x.Description, // Lấy toàn bộ nội dung
                    CreatedDate = x.StartDate.Value
                })
                .FirstOrDefaultAsync();

            return newsDetail;
        }
    }
}
