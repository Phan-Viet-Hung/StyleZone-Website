using API.DomainCusTomer.DTOs;
using API.DomainCusTomer.DTOs.Color;
using API.DomainCusTomer.DTOs.DetailCustomer;
using API.DomainCusTomer.DTOs.TheThao;
using API.DomainCusTomer.ExTentions;
using API.DomainCusTomer.Request.TheThao;
using API.DomainCusTomer.Services.IServices;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API.DomainCusTomer.Services
{
    public class DetailCustomerServices : IDetailCustomerServices
    {
        private readonly DbContextApp _context;

        public DetailCustomerServices(DbContextApp context)
        {
            _context = context;
        }
        public async Task<DetailCustomerDto> GetId(Guid id)
        {
            // Lấy detail sản phẩm
            var detail = await _context.ProductDetails
                .Include(p => p.Color)
                .Include(p => p.Size)
                .Include(p => p.Material)
                .Include(p => p.Images)
                .Include(p => p.Origin)
                .Include(p => p.Supplier)
                .Include(p => p.Product).ThenInclude(p => p.Category)
                .Include(p => p.Product).ThenInclude(p => p.Brand)
                .Include(p => p.PromotionProducts).ThenInclude(p => p.Promotion)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (detail == null) return null;

            // Convert sang DTO
            var dto = detail.ToDetailCustomerDto();

            // ===================== Sản phẩm cùng danh mục =====================
            dto.ProductDetailsSameCategory = await _context.ProductDetails
                .Include(p => p.Images)
                .Where(p => p.Product.CategoryId == detail.Product.CategoryId && p.Status == ProductDetailStatus.Active
                && p.Quantity > 0)
                .Select(p => new PageDetail
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.Images.FirstOrDefault().Url ?? "default.jpg"
                })
                .ToListAsync();

            // ===================== Kiểm tra biến thể =====================
            bool hasVariants = await _context.ProductDetails
                .AnyAsync(p => p.ProductId == detail.ProductId && (p.ColorId != detail.ColorId || p.SizeId != detail.SizeId));

            if (!hasVariants)
                return dto;  // Không có biến thể => return luôn

            // ===================== Build danh sách màu =====================
            dto.AllColors = await _context.ProductDetails
            .Where(p => p.ProductId == detail.ProductId
                  && p.Color != null
                  && p.Status == ProductDetailStatus.Active
                  && p.Quantity > 0)
                  .GroupBy(p => new { p.Color.Id, p.Color.Name, p.Color.Code })
                .Select(g => new ColorDto
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    Code = g.Key.Code
                })
                .ToListAsync();

            // ===================== Build danh sách size =====================
            dto.AllSizes = await _context.ProductDetails
            .Where(p => p.ProductId == detail.ProductId
                 && p.Size != null
                 && p.Status == ProductDetailStatus.Active
                 && p.Quantity > 0)
                 .GroupBy(p => new { p.Size.Id, p.Size.Code, p.Size.Name })
                .Select(g => new SizeDto
                {
                    Id = g.Key.Id,
                    Code = g.Key.Code,
                    Name = g.Key.Name
                })
                .ToListAsync();

            // ===================== Build ColorSizeMap & SizeColorMap =====================
            var productDetails = await _context.ProductDetails
                .Where(p => p.ProductId == detail.ProductId)
                .Select(p => new { p.ColorId, p.SizeId })
                .ToListAsync();

            dto.ColorSizeMap = productDetails
                .GroupBy(x => x.ColorId)
       .ToDictionary(
                    g => g.Key ?? Guid.Empty,
                    g => g.Select(x => x.SizeId ?? Guid.Empty).Distinct().ToList()
                );

            dto.SizeColorMap = productDetails
                .GroupBy(x => x.SizeId)
                .ToDictionary(
                    g => g.Key ?? Guid.Empty,
                    g => g.Select(x => x.ColorId ?? Guid.Empty).Distinct().ToList()
                );

            // ===================== Build ProductVariantMap =====================
            var variantList = await _context.ProductDetails
                .Where(p => p.ProductId == detail.ProductId && p.ColorId != null && p.SizeId != null)
                .Select(p => new
                {
                    ColorId = p.ColorId,
                    SizeId = p.SizeId,
                    Code = p.Code ?? string.Empty
                })
                .ToListAsync();
            dto.ProductVariantMap = variantList
                .GroupBy(v => $"{v.ColorId}-{v.SizeId}")
                .ToDictionary(
                    g => g.Key,
                    g => g.First().Code
                );

            return dto;
        }



    }
}
