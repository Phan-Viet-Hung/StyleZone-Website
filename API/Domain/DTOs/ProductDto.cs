using DAL_Empty.Models;
using System.Text.Json.Serialization;
namespace API.Domain.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public GenderEnum Gender { get; set; }
        public string GenderName => Gender switch
        {
            GenderEnum.Nam => "Nam",
            GenderEnum.Nu => "Nữ",
            GenderEnum.Khac => "Khác"
        };
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;

        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = null!;
        public int TotalQuantity { get; set; }
        public string? MainImageUrl =>
            ProductDetails?
            .FirstOrDefault(pd => pd.Images != null && pd.Images.Any(i => !string.IsNullOrWhiteSpace(i.Url)))
            ?.Images
            .FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.Url))
            ?.Url;

        public List<ProductDetailDto> ProductDetails { get; set; }

    }


}

