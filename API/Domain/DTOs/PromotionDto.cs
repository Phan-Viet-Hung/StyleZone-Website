namespace API.Domain.DTOs
{
    public class PromotionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }

        public string? ImageUrl { get; set; }
        public List<string>? ProductNames { get; set; }
        public List<Guid>? ProductDetailIds { get; set; }
        public int ProductCount { get; set; }


    }
}
