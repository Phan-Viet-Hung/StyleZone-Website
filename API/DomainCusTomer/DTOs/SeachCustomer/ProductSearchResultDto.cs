namespace API.DomainCusTomer.DTOs.SeachCustomer
{
    public class ProductSearchResultDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; } // Giá sau khi giảm, có thể null
        public string ImageUrl { get; set; }
        public string ImageUrlHover { get; set; }
        public string CategoryName { get; set; }
    }
}
