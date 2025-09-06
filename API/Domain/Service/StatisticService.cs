using API.Domain.DTOs.ThongKe;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.Service
{
    public class StatisticService : IStatisticService
    {
        private readonly DbContextApp _context;

        public StatisticService(DbContextApp context)
        {
            _context = context;
        }

        // Helper method để xử lý date filter logic
        private (DateTime startDate, DateTime endDate) ProcessDateFilter(DateFilterDto filter)
        {
            DateTime endDate;
            DateTime startDate;

            // Xử lý filterType
            switch (filter.FilterType)
            {
                case "7days":
                    endDate = DateTime.Now;
                    startDate = endDate.AddDays(-6);
                    break;

                case "thisMonth":
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    endDate = DateTime.Now;
                    break;

                case "last3Months":
                    endDate = DateTime.Now;
                    startDate = endDate.AddMonths(-3);
                    break;

                case "1year":
                    endDate = DateTime.Now;
                    startDate = endDate.AddYears(-1);
                    break;

                case "custom":
                    if (!filter.StartDate.HasValue || !filter.EndDate.HasValue)
                    {
                        throw new ArgumentException("Vui lòng nhập đầy đủ ngày bắt đầu và ngày kết thúc.");
                    }
                    startDate = filter.StartDate.Value.Date;
                    endDate = filter.EndDate.Value.Date;
                    break;

                default:
                    endDate = DateTime.Now;
                    startDate = endDate.AddDays(-6);
                    break;
            }

            // ✅ Validate ngày
            if (startDate > endDate)
            {
                throw new ArgumentException("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            }

            return (startDate, endDate);
        }

        public async Task<DashboardStatisticDto> GetDashboardStatisticsAsync(DateFilterDto filter)
        {
            var (startDate, endDate) = ProcessDateFilter(filter);

            DateTime from = startDate.Date;
            DateTime to = endDate.Date.AddDays(1).AddTicks(-1);

            // Lấy thống kê tổng
            var totalProducts = await _context.Products.CountAsync();
            var totalOrders = await _context.OrderInfos.CountAsync();

            var totalCategories = await _context.Categories.CountAsync();
            var totalCustomers = await _context.Customers
    .Where(c => c.Status != null && c.Status.ToLower() == "active")
    .CountAsync();

            // ✅ Nếu lọc custom mà không có đơn hàng
            if (filter.FilterType == "custom" && totalOrders == 0)
            {
                throw new ArgumentException("Không tìm thấy đơn hàng nào trong khoảng thời gian này.");
            }

            // Dữ liệu biểu đồ
            var revenueData = await _context.OrderDetails
                .Where(od => od.Order != null
                    && od.Order.CreateAt.HasValue
                    && od.Order.CreateAt.Value >= from
                    && od.Order.CreateAt.Value <= to
                    && od.Order.Status == OrderStatus.Delivered)
                .GroupBy(od => od.Order.CreateAt.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity ?? 0),
                    TotalRevenue = g.Sum(x => (x.Quantity ?? 0) * (x.Price ?? 0))
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // ✅ Nếu lọc custom mà biểu đồ không có dữ liệu
            if (filter.FilterType == "custom" && !revenueData.Any())
            {
                throw new ArgumentException("Không tìm thấy dữ liệu doanh thu trong khoảng thời gian này.");
            }

            return new DashboardStatisticDto
            {
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalCategories = totalCategories,
                TotalUsers = totalCustomers,
                ChartData = revenueData.Select(d => new RevenueStatisticDto
                {
                    TimeFrame = d.Date.ToString("yyyy-MM-dd"),
                    TotalQuantitySold = d.TotalQuantity,
                    TotalRevenue = d.TotalRevenue
                }).ToList(),
            };
        }

        public async Task<List<OrderStatusStatisticDto>> GetOrderStatusStatisticsAsync(DateFilterDto filter)
        {
            var (startDate, endDate) = ProcessDateFilter(filter);
            DateTime from = startDate.Date;
            DateTime to = endDate.Date.AddDays(1).AddTicks(-1);

            // Lấy tất cả trạng thái trong enum
            var allStatuses = Enum.GetValues(typeof(OrderStatus))
                                  .Cast<OrderStatus>().ToList();

            // Truy vấn đếm số lượng đơn theo status trong khoảng thời gian lọc
            var data = await _context.OrderInfos
                .Where(o => o.CreateAt >= from && o.CreateAt <= to)
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Map tất cả trạng thái, kể cả những trạng thái không có đơn
            var result = allStatuses.Select(status => new OrderStatusStatisticDto
            {
                Status = status,
                StatusName = GetVietnameseStatusName(status),
                TotalOrders = data.FirstOrDefault(d => d.Status == status)?.Count ?? 0
            }).ToList();

            // Validate nếu filter là custom và không có dữ liệu
            if (filter.FilterType == "custom" && result.All(r => r.TotalOrders == 0))
            {
                throw new ArgumentException("Không tìm thấy đơn hàng nào trong khoảng thời gian này.");
            }

            return result;
        }

        // ✅ Hàm chuyển trạng thái sang tiếng Việt
        private string GetVietnameseStatusName(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Chờ xử lý",
                OrderStatus.Confirmed => "Đã xác nhận",
                OrderStatus.Processing => "Đang xử lý",
                OrderStatus.Shipping => "Đang giao hàng",
                OrderStatus.Delivered => "Đã giao hàng",
                OrderStatus.Cancelled => "Đã hủy",
                
                _ => "Không xác định"
            };
        }


        public async Task<List<TopBrandDto>> GetTopBrandsAsync(DateFilterDto filter, int top = 3)
        {
            // Validate tham số
            if (top <= 0)
                throw new ArgumentException("Số lượng top phải lớn hơn 0", nameof(top));

            var (startDate, endDate) = ProcessDateFilter(filter);
            DateTime from = startDate.Date;
            DateTime to = endDate.Date.AddDays(1).AddTicks(-1);

            var data = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.ProductDetail)
                    .ThenInclude(pd => pd.Product)
                .Where(od =>
                    od.Order != null &&
                    od.Order.CreateAt.HasValue &&
                    od.Order.CreateAt.Value >= from &&
                    od.Order.CreateAt.Value <= to &&
                    od.Order.Status == OrderStatus.Delivered &&
                    od.ProductDetail != null &&
                    od.ProductDetail.Product != null)
                .GroupBy(od => new
                {
                    BrandId = od.ProductDetail.Product.BrandId,
                    BrandName = od.ProductDetail.Product.Brand != null
                        ? od.ProductDetail.Product.Brand.Name
                        : "Không xác định"
                })
                .Select(g => new TopBrandDto
                {
                    BrandId = g.Key.BrandId,
                    BrandName = g.Key.BrandName,
                    TotalSold = g.Sum(x => (int)(x.Quantity ?? 0))
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(top)
                .ToListAsync();

            // ✅ Validation cho custom filter
            if (filter.FilterType == "custom" && !data.Any())
            {
                throw new ArgumentException("Không tìm thấy dữ liệu thương hiệu trong khoảng thời gian này.");
            }

            return data ?? new List<TopBrandDto>();
        }

        public async Task<List<TopProductDto>> GetTopProductsAsync(DateFilterDto filter, int top = 10)
        {
            // Validate tham số
            if (top <= 0)
                throw new ArgumentException("Số lượng top phải lớn hơn 0", nameof(top));

            var (startDate, endDate) = ProcessDateFilter(filter);
            DateTime from = startDate.Date;
            DateTime to = endDate.Date.AddDays(1).AddTicks(-1);

            var data = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.ProductDetail)
                    .ThenInclude(pd => pd.Product)
                .Where(od =>
                    od.Order != null &&
                    od.Order.CreateAt.HasValue &&
                    od.Order.CreateAt.Value >= from &&
                    od.Order.CreateAt.Value <= to &&
                    od.Order.Status == OrderStatus.Delivered &&
                    od.ProductDetail != null &&
                    od.ProductDetail.Product != null)
                .GroupBy(od => new
                {
                    ProductId = od.ProductDetail.Product.Id,
                    ProductName = od.ProductDetail.Product.Name
                })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName ?? "Không xác định",
                    TotalSold = g.Sum(x => x.Quantity ?? 0)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(top)
                .ToListAsync();

            // ✅ Validation cho custom filter
            if (filter.FilterType == "custom" && !data.Any())
            {
                throw new ArgumentException("Không tìm thấy dữ liệu sản phẩm trong khoảng thời gian này.");
            }

            return data ?? new List<TopProductDto>();
        }
    }
}