namespace API.Domain.DTOs
{
    public class PromotionDetailDto
    {
        public PromotionDto? Promotion { get; set; }
        public List<Guid> SelectedProductIds { get; set; } = new();
        public List<PromotionProductDto> PromotionProducts { get; set; } = new();

    }
}
