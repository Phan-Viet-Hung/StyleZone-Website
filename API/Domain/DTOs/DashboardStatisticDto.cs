    namespace API.Domain.DTOs.ThongKe
{
    public class DashboardStatisticDto
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
        public List<RevenueStatisticDto> ChartData { get; set; }
        public List<TopBrandDto> TopBrands { get; set; }
        public List<TopProductDto> TopProducts { get; set; }
    }

 

    public class DateFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string FilterType { get; set; } // 7days, thisMonth, last3Months, 1year, custom
    }

}
