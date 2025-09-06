using API.DomainCusTomer.DTOs.SeachCustomer;
using API.DomainCusTomer.Services.IServices;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.DomainCusTomer.Services
{
    public class SeachCustomerService:ISeachCustomerService
    {
        private readonly DbContextApp _context; 
        public SeachCustomerService(DbContextApp context)
        {
            _context = context;
        }

        public async Task<List<ProductSearchResultDto>> SearchProductsAsync(string? keyword)
        {
            var now = DateTime.Now;
            var query = _context.ProductDetails
                .Include(pd => pd.Product).ThenInclude(p => p.Category)
                .Include(pd => pd.Images)
                .Include(pd => pd.PromotionProducts).ThenInclude(pp => pp.Promotion)
                .Where(pd => pd.Status == ProductDetailStatus.Active && pd.Quantity > 0);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(pd => pd.Name.ToLower().Contains(lowerKeyword) ||
                                           pd.Product.Name.ToLower().Contains(lowerKeyword));
            }

            return await query
                .OrderByDescending(pd => pd.Product.CreatedAt)
                .Select(pd => new ProductSearchResultDto
                {
                    Id = pd.Id,
                    Name = pd.Name,
                    Price = pd.Price,
                    ImageUrl = pd.Images.Select(i => i.Url).FirstOrDefault() ?? "",
                    ImageUrlHover = pd.Images.Select(i => i.Url).Skip(1).FirstOrDefault() ?? "",
                    CategoryName = pd.Product.Category.Name,
                    DiscountPrice = pd.PromotionProducts
                        .Where(p => p.Promotion != null && p.Promotion.StartDate <= now && p.Promotion.EndDate >= now && p.Promotion.Status == VoucherStatus.Active)
                        .OrderByDescending(p => p.Promotion.StartDate )
                        .Select(p => (decimal?)p.Priceafterduction)
                        .FirstOrDefault()
                })

                .ToListAsync();
        }
    }
}
