using API.Domain.Request.ProductDetailRequest;
using API.Domain.Request.ProductRequest;
using DAL_Empty.Models;

namespace API.Domain.Mappers
{
    public static class MapperHelper
    {
        public static Product MapCreateProductRequestToProduct(CreateProductRequest req)
        {
            return new Product
            {
                Id = Guid.NewGuid(),  // Tạo mới Guid
                Name = req.Name,
                Description = req.Description,
                Gender = req.Gender,
                CreatedAt = DateTime.Now,
                CategoryId = req.CategoryId,
                BrandId = req.BrandId,
                UpdatedAt = null,
                UpdatedBy = null,
            };
        }
        public static ProductDetail MapCreateRequestToEntity(CreateProductDetailRequest req)
        {
            return new ProductDetail
            {
                Id = Guid.NewGuid(),
                Code = req.Code,
                Price = req.Price,
                Quantity = req.Quantity,
                ProductId = req.ProductId,
                ColorId = req.ColorId,
                SizeId = req.SizeId,
                MaterialId = req.MaterialId,
                OriginId = req.OriginId,
                SupplierId = req.SupplierId,
                Status = req.Status,
                // Các navigation property để null lúc tạo
            };

        }

    }
}