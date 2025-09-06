namespace API.Domain.DTOs.ThongKe
{
    public class RevenueStatisticDto
    {
        public string TimeFrame { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
