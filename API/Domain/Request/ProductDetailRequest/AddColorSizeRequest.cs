namespace API.Domain.Request.ProductDetailRequest
{
    public class AddColorSizeRequest
    {
        public Guid ProductDetailsId { get; set; }
        public Guid ColorId { get; set; }
        public Guid SizeId { get; set; }

    }
}
