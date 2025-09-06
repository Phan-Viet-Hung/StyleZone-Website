
using API.Domain.DTOs;
using DAL_Empty.Models;

namespace API.Domain.Extentions
{
    public static class ProductExtensions
    {
        public static ProductDto ToDto(this Product product)
        {
            if (product == null) return null!;

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Gender = product.Gender,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                CreatedBy = product.CreatedBy,
                UpdatedBy = product.UpdatedBy,
                CategoryId = product.CategoryId ,
                BrandId = product.BrandId,
                CategoryName = product.Category?.Name ?? "",
                BrandName = product.Brand?.Name ?? "",

            };
        }
    }
}
