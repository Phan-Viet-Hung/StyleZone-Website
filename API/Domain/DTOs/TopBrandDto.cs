namespace API.Domain.DTOs.ThongKe
{
    public class TopBrandDto
    {
        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
    }
}
