namespace API.DomainCusTomer.DTOs.TrangChu
{
    public class HomeProductCustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string ImageUrlHover { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string CategoryName { get; set; }
        public string? ShortDescription { get; set; }
    }
}
