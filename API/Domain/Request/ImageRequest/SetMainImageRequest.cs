namespace API.Domain.Request.ImageRequest
{
    public class SetMainImageRequest
    {
        public Guid ImageId { get; set; }
        public Guid ProductDetailId { get; set; }

    }
}
