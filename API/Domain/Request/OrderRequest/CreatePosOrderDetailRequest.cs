namespace API.Domain.Request.OrderRequest
{
    public class CreatePosOrderDetailRequest
    {
        public Guid ProductDetailId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
