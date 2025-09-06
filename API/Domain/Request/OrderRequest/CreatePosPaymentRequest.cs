namespace API.Domain.Request.OrderRequest
{
    public class CreatePosPaymentRequest
    {
        public Guid PaymentMethodId { get; set; }

        public decimal PaymentAmount { get; set; }
    }
}
