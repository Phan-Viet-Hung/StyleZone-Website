namespace API.Domain.Request.OrderRequest
{
    public class CreateOrderRequest
    {
        public string? CustomerName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;

        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Description { get; set; }
        public List<CreatePosModeOfPayment> ModeOfPayments { get; set; }
        public List<CreatePosOrderDetailRequest> OrderDetails { get; set; } = new();
        //public List<CreatePosPaymentRequest> Payments { get; set; } = new();
    }
}
