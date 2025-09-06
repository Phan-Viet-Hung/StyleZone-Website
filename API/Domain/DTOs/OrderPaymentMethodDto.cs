namespace API.Domain.DTOs
{
    public class OrderPaymentMethodDto
    {
        public Guid Id { get; set; }
        public Guid? PaymentMethodId { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string? PaymentMethodName { get; set; }
    }
}
