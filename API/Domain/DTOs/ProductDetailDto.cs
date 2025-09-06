using DAL_Empty.Models;
namespace API.Domain.DTOs
{
    public class ProductDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Code { get; set; } = string.Empty;

        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;

        public Guid? ColorId { get; set; }
        public string ColorName { get; set; } = string.Empty;

        public Guid? SizeId { get; set; }
        public string SizeName { get; set; } = string.Empty;

        public Guid? MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;

        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public Guid? BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;

        public Guid? OriginId { get; set; }
        public string OriginName { get; set; } = string.Empty;

        public Guid? SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;

        public string StatusName { get; set; }
        public decimal? DisplayPrice { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public string? MainImageUrl { get; set; }
        public List<ImageDto> Images { get; set; } = new();


    }

}
