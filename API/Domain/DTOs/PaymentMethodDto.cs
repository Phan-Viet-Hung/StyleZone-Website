namespace API.Domain.DTOs
{
    public class PaymentMethodDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
