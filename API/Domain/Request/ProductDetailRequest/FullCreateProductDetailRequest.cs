namespace API.Domain.Request.ProductDetailRequest
{
    public class FullCreateProductDetailRequest
    {
        public CreateProductDetailRequest ProductDetail { get; set; } = null!;
        public List<IFormFile> Images { get; set; } = new();
        public string? MainImageFileName { get; set; }  // Tên file nào sẽ là ảnh chính


    }
}
 