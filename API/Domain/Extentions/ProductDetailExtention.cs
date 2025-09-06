using API.Domain.DTOs;
using DAL_Empty.Models;
using System.Linq;

namespace API.Domain.Extentions
{
    public static class ProductDetailExtensions
    {
        public static ProductDetailDto ToDto(this ProductDetail p)
        {
            return new ProductDetailDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Quantity = p.Quantity ?? 0,
                Code = p.Code ?? string.Empty,

                ProductId = p.ProductId,
                ProductName = p.Product?.Name ?? string.Empty,
                Gender = p.Product?.Gender.ToString() ?? string.Empty,

                ColorId = p.ColorId,
                ColorName = p.Color?.Name ?? string.Empty,

                SizeId = p.SizeId,
                SizeName = p.Size?.Name ?? string.Empty,

                MaterialId = p.MaterialId,
                MaterialName = p.Material?.Name ?? string.Empty,

                CategoryId = p.Product?.CategoryId,
                CategoryName = p.Product?.Category?.Name ?? string.Empty,

                BrandId = p.Product?.BrandId,
                BrandName = p.Product?.Brand?.Name ?? string.Empty,

                OriginId = p.OriginId,
                OriginName = p.Origin?.Name ?? string.Empty,

                SupplierId = p.SupplierId,
                SupplierName = p.Supplier?.Name ?? string.Empty,

                StatusName = p.Status.ToString(),

                // Vẫn giữ lại để tiện dùng nhanh
                MainImageUrl = p.Images?.FirstOrDefault(i => i.IsMainImage)?.Url,
                ImageUrls = p.Images?.Select(x => x.Url).ToList() ?? new List<string>(),

                // Thêm mapping đầy đủ
                Images = p.Images?.Select(i => new ImageDto
                {
                    Id = i.Id,
                    Url = i.Url,
                    IsMainImage = i.IsMainImage
                }).ToList() ?? new List<ImageDto>()
            };
        }

    }
}
