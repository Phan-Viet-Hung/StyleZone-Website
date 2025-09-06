using API.Domain.DTOs;
using DAL_Empty.Models;
namespace API.Domain.Extentions
{
    public static class ColorExtentions
    {
        public static ColorDto ToDto(this Color color)
        {
            return new ColorDto
            {
                Id = color.Id,
                Code = color.Code,
                Name = color.Name ?? "",
                CreatedAt = color.CreatedAt,
                UpdatedAt = color.UpdatedAt,
                QuantityProduct = color.ProductDetails?.Count ?? 0,
                TotalQuantity = color.ProductDetails?.Sum(pd => pd.Quantity) ?? 0
            };
        }
    }

}
