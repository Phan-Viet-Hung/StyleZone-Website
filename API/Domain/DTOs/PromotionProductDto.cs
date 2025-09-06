namespace API.Domain.DTOs
{
    public class PromotionProductDto
    {
        public Guid Id { get; set; }
        public Guid ProductDetailId { get; set; }
        public decimal PriceBeforeDiscount { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public bool IsActive { get; set; }
    }
}
