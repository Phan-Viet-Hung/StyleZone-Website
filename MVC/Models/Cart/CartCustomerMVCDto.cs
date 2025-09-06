namespace MVC.Models.Cart
{
    public class CartCustomerMVCDto
    {
        public string ProductDetailcode { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? ColorName { get; set; }
        public string? ColorCode { get; set; }
        public string? SizeName { get; set; }
    }
}
