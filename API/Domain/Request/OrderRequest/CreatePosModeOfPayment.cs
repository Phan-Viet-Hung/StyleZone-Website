namespace API.Domain.Request.OrderRequest
{
    public class CreatePosModeOfPayment
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
