using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.ProductDetailRequest
{
    public class ImportExcelProductDetailRequest
    {
        [Required]
        public IFormFile File { get; set; } = default!;

        [Required]
        public Guid ProductId { get; set; }
    }
}
