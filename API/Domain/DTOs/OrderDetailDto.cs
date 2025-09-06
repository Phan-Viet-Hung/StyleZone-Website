namespace API.Domain.DTOs
{
    public class OrderDetailDto
    {
        public Guid Id { get; set; }
        public Guid? ProductDetailId { get; set; }
        public string? ProductName { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
    }
}
