using API.DomainCusTomer.DTOs.TrangChu;
using API.DomainCusTomer.Services.IServices;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.DomainCusTomer.Services
{
    public class TrangChuCustomerService : ITrangChuCustomerService
    {
        private readonly DbContextApp _context;
        public TrangChuCustomerService(DbContextApp context)
        {
            _context = context;
        }
        public async Task<Dictionary<string, List<HomeProductCustomerDto>>> GetSanPhamTrangChu()
        {
            var now = DateTime.Now;

            // 1. Sản phẩm mới ra mắt
            var newProducts = await _context.ProductDetails
                .Include(pd => pd.Product).ThenInclude(p => p.Category)
                .Include(pd => pd.Images)
                .Include(pd => pd.PromotionProducts).ThenInclude(pp => pp.Promotion)
                .Where(pd => pd.Status == ProductDetailStatus.Active && pd.Quantity > 0)
                .OrderByDescending(pd => pd.Product.CreatedAt)
                .Take(12)
                .Select(pd => new HomeProductCustomerDto
                {
                    Id = pd.Id,
                    Name = pd.Name,
                    Price = pd.Price,
                    ImageUrl = pd.Images.Select(i => i.Url).FirstOrDefault() ?? "",
                    ImageUrlHover = pd.Images.Select(i => i.Url).Skip(1).FirstOrDefault() ?? "",
                    DiscountPrice = pd.PromotionProducts
                        .Where(p => p.Promotion != null && p.Promotion.StartDate <= now && p.Promotion.EndDate >= now && p.Promotion.Status == VoucherStatus.Active)
                        .OrderByDescending(p => p.Promotion.StartDate)
                        .Select(p => (decimal?)p.Priceafterduction)
                        .FirstOrDefault(),
                    CategoryName = pd.Product.Category.Name
                }).ToListAsync();

            // 2. Sản phẩm bán chạy
            var bestSeller = await _context.OrderDetails
                .Where(o => o.ProductDetail != null && o.ProductDetail.Status == ProductDetailStatus.Active && o.ProductDetail.Quantity > 0)
                .GroupBy(o => o.ProductDetailId)
                .Select(g => new
                {
                    ProductDetailId = g.Key,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(12)
                .ToListAsync();

            var productDetailIds = bestSeller.Select(x => x.ProductDetailId).ToList();

            var bestSellerDetails = await _context.ProductDetails
                .Where(pd => productDetailIds.Contains(pd.Id))
                .Include(pd => pd.Product).ThenInclude(p => p.Category)
                .Include(pd => pd.Images)
                .Include(pd => pd.PromotionProducts).ThenInclude(pp => pp.Promotion)
                .ToListAsync();

            var bestSellerProducts = bestSeller.Select(x =>
            {
                var pd = bestSellerDetails.First(p => p.Id == x.ProductDetailId);
                return new HomeProductCustomerDto
                {
                    Id = pd.Id,
                    Name = pd.Name,
                    Price = pd.Price,
                    ImageUrl = pd.Images.Select(i => i.Url).FirstOrDefault() ?? "",
                    ImageUrlHover = pd.Images.Select(i => i.Url).Skip(1).FirstOrDefault() ?? "",
                    DiscountPrice = pd.PromotionProducts
                        .Where(p => p.Promotion != null && p.Promotion.StartDate <= now && p.Promotion.EndDate >= now && p.Promotion.Status == VoucherStatus.Active)
                        .OrderByDescending(p => p.Promotion.StartDate)
                        .Select(p => (decimal?)p.Priceafterduction)
                        .FirstOrDefault(),
                    CategoryName = pd.Product.Category.Name
                };
            }).ToList();

            var promotions = await _context.Promotions
               .Where(p => p.Status == VoucherStatus.Active
                           && p.StartDate <= now
                           && p.EndDate >= now)
               .OrderByDescending(p => p.StartDate)
               .Take(3)
               .Select(p => new HomeProductCustomerDto
               {
                   Id = p.Id,
                   Name = p.Name,
                   ImageUrl = p.ImageUrl, // <-- Chỉ trả về tên file ảnh
                   Price = 0,
                   DiscountPrice = null,
                   ImageUrlHover = "",
                   CategoryName = p.Description
               })
               .ToListAsync();

            return new Dictionary<string, List<HomeProductCustomerDto>>
            {
                { "Sản phẩm mới ra mắt", newProducts },
                { "Sản phẩm bán chạy", bestSellerProducts },
                { "Tin tức khuyến mãi", promotions }
            };
        }
    }
}
