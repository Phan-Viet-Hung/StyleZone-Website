using API.Domain.DTOs;
using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.ProductDetailRequest
{
    public class UpdateProductDetailRequest : CreateProductDetailRequest
    {
        public Guid Id { get; set; }
        public List<ImageDto> ExistingImages { get; set; } = new();

    }
}
