namespace API.Domain.DTOs.ThongKe
{
    public class TopProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
    }
}
