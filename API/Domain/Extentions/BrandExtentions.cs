using API.Domain.DTOs;
using DAL_Empty.Models;

namespace API.Domain.Extentions
{
    public static class BrandExtentions
    {
        public static BrandDto ToDto(this Brand brand)
        {
            if (brand == null) return null!;
            return new BrandDto
            {
                Id = brand.Id,
                Code = brand.Code,
                Name = brand.Name,
                CreatedAt = brand.CreatedAt,
                UpdatedAt = brand.UpdatedAt,
                QuantityProduct = brand.Product?.Count ?? 0
            };
        }
    }
}
