using API.Domain.DTOs;
using API.Domain.Extentions;
using API.Domain.Request.ColorRequest;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.Service
{
    public class ColorService : IColorService
    {
        private readonly DbContextApp _context;

        public ColorService(DbContextApp context)
        {
            _context = context;
        }

        //public async Task<ColorDto> CreateAsync(CreateColorRequest request)
        //{
        //    string name = request.Name.Trim();
        //    string? code = request.Code?.Trim();

        //    if (await _context.Colors.AnyAsync(c => c.Name == name || (code != null && c.Code == code)))
        //        throw new Exception("Màu đã tồn tại với tên hoặc mã trùng.");

        //    var color = new Color
        //    {
        //        Id = Guid.NewGuid(),
        //        Name = name,
        //        Code = code,
        //        CreatedAt = DateTime.Now
        //    };

        //    _context.Colors.Add(color);
        //    await _context.SaveChangesAsync();

        //    return color.ToDto();
        //}
        public async Task<ColorDto> CreateAsync(CreateColorRequest request)
        {
            string name = request.Name?.Trim().ToLowerInvariant() ?? "";
            string? code = request.Code?.Trim().ToUpperInvariant(); // Chuẩn hóa về in hoa

            // Kiểm tra trùng tên (bỏ qua chữ hoa thường)
            if (await _context.Colors.AnyAsync(c => c.Name.ToLower() == name))
                throw new Exception("Tên màu đã tồn tại.");

            // Kiểm tra trùng mã (bỏ qua chữ hoa thường)
            if (!string.IsNullOrWhiteSpace(code))
            {
                if (await _context.Colors.AnyAsync(c => c.Code.ToUpper() == code))
                    throw new Exception("Mã màu đã tồn tại.");
            }

            var color = new Color
            {
                Id = Guid.NewGuid(),
                Name = request.Name?.Trim(),
                Code = code,
                CreatedAt = DateTime.Now
            };

            _context.Colors.Add(color);
            await _context.SaveChangesAsync();

            return color.ToDto();
        }

        public async Task<ColorDto> UpdateAsync(UpdateColorRequest request)
        {
            var color = await _context.Colors.FindAsync(request.Id);
            if (color == null)
                throw new Exception("Không tìm thấy màu cần cập nhật.");

            string newName = request.Name.Trim();
            string nameToCompare = newName.ToLowerInvariant();
            string? newCode = request.Code?.Trim();
            string? codeToCompare = newCode?.ToUpperInvariant();

            // Kiểm tra trùng tên (không phân biệt hoa thường)
            bool isDuplicateName = await _context.Colors.AnyAsync(c =>
                c.Id != request.Id && c.Name.ToLower() == nameToCompare);

            if (isDuplicateName)
                throw new Exception("Đã tồn tại màu khác với tên trùng.");

            // Kiểm tra trùng mã màu (nếu có nhập mã, không phân biệt hoa thường)
            if (!string.IsNullOrWhiteSpace(codeToCompare))
            {
                bool isDuplicateCode = await _context.Colors.AnyAsync(c =>
                    c.Id != request.Id && c.Code != null && c.Code.ToUpper() == codeToCompare);

                if (isDuplicateCode)
                    throw new Exception("Đã tồn tại màu khác với mã trùng.");
            }

            // Gán giá trị mới
            color.Name = newName;
            color.Code = newCode;
            color.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return color.ToDto();
        }


        //public async Task<ColorDto> UpdateAsync(UpdateColorRequest request)
        //{
        //    var color = await _context.Colors.FindAsync(request.Id);
        //    if (color == null)
        //        throw new Exception("Không tìm thấy màu cần cập nhật.");

        //    string newName = request.Name.Trim();
        //    string? newCode = request.Code?.Trim();

        //    // Kiểm tra trùng mã/tên với màu khác
        //    bool isDuplicate = await _context.Colors.AnyAsync(c =>
        //        c.Id != request.Id && (c.Name == newName || (newCode != null && c.Code == newCode)));

        //    if (isDuplicate)
        //        throw new Exception("Đã tồn tại màu khác với mã hoặc tên trùng.");

        //    color.Name = newName;
        //    color.Code = newCode;
        //    color.UpdatedAt = DateTime.Now;
        //    await _context.SaveChangesAsync();
        //    return color.ToDto();
        //}

        //public async Task<List<ColorDto>> GetAllAsync()
        //{
        //    var colors = await _context.Colors.Include(c => c.ProductDetails).ToListAsync();
        //    return colors.Select(c => c.ToDto()).ToList();
        //}
        public async Task<List<ColorDto>> GetAllAsync()
        {
            var colors = await _context.Colors
                .Select(color => new ColorDto
                {
                    Id = color.Id,
                    Code = color.Code,
                    Name = color.Name,
                    CreatedAt = color.CreatedAt,
                    UpdatedAt = color.UpdatedAt,

                    // Đếm số lượng sản phẩm (ProductDetail) có màu này
                    QuantityProduct = _context.ProductDetails.Count(pd => pd.ColorId == color.Id),

                    // Tính tổng số lượng từ các sản phẩm có màu này
                    TotalQuantity = _context.ProductDetails
                        .Where(pd => pd.ColorId == color.Id)
                        .Sum(pd => (int?)pd.Quantity) ?? 0
                })
                .ToListAsync();

            return colors;
        }

        public async Task<ColorDto?> GetByIdAsync(Guid id)
        {
            var color = await _context.Colors.FindAsync(id);
            return color?.ToDto();
        }
    }
}
