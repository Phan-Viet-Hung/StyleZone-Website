using API.DomainCusTomer.DTOs;
using DAL_Empty.Models;

namespace API.DomainCusTomer.ExTentions
{
    public static class ProductDetailCustomerExtensions
    {
        public static ProductDetailCustomerDto ToCustomerDto(this ProductDetail p)
        {
            return new ProductDetailCustomerDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Quantity = p.Quantity ?? 0,
                Gender = p.Product.Gender,
                CodeProductDetail = p.Code,


                ProductId = p.ProductId,
                ProductName = p.Product?.Name ?? string.Empty,
                DateTime = p.Product.CreatedAt,
                ProductDescription = p.Product.Description ?? string.Empty,

                ColorId = p.ColorId,
                ColorName = p.Color?.Name ?? string.Empty,
                ColorCode = p.Color?.Code ?? string.Empty,

                SizeId = p.SizeId,
                Code = p.Size?.Code ?? string.Empty,

                MaterialId = p.MaterialId,
                MaterialName = p.Material?.Name ?? string.Empty,

                CategoryId = p.Product.CategoryId,
                CategoryName = p.Product.Category.Name ?? string.Empty,

                BrandId = p.Product.BrandId,
                BrandName = p.Product.Category?.Name ?? string.Empty,

                OriginId = p.OriginId,
                OriginName = p.Origin?.Name ?? string.Empty,

                SupplierId = p.SupplierId,
                SupplierName = p.Supplier?.Name ?? string.Empty,
                ImageUrls = p.Images.Select(x => x.Url).ToList() ?? new List<string>(),


                PromotionProductId = p.PromotionProducts?.Select(x => (Guid?)x.Id).ToList() ?? new List<Guid?>(),

                PromotionId = p.PromotionProducts?.Select(x => x.Promotion?.Id.ToString() ?? string.Empty).ToList() ?? new List<string>(),

                PromotionName = p.PromotionProducts?.Select(x => x.Promotion?.Name ?? string.Empty).ToList() ?? new List<string>(),

                StartDate = p.PromotionProducts?.Select(x => x.Promotion.StartDate.ToString() ?? string.Empty).ToList() ?? new List<string>(),

                EndDate = p.PromotionProducts?.Select(x => x.Promotion?.EndDate.ToString() ?? string.Empty).ToList() ?? new List<string>(),

                Status = p.PromotionProducts?.Select(x => x.Promotion?.Status ?? VoucherStatus.Active)  .ToList() ?? new List<VoucherStatus>(),



                PriceBeforeReduction = p.PromotionProducts?.Select(x => x.Pricebeforereduction).ToList() ?? new List<decimal>(),

                Priceafterduction = p.PromotionProducts?.Select(x => x.Priceafterduction).ToList() ?? new List<decimal>(),

            };
        }
    }
}
