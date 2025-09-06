using API.Domain.DTOs.ThongKe;

namespace API.Domain.Service.IService
{
    public interface IStatisticService
    {
        Task<DashboardStatisticDto> GetDashboardStatisticsAsync(DateFilterDto filter);
        Task<List<TopBrandDto>> GetTopBrandsAsync(DateFilterDto filter, int top = 3);
        Task<List<TopProductDto>> GetTopProductsAsync(DateFilterDto filter, int top = 10);
        Task<List<OrderStatusStatisticDto>> GetOrderStatusStatisticsAsync(DateFilterDto filter);
    }

}
