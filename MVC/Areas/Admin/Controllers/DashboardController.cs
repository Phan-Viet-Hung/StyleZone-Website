using API.Domain.DTOs.ThongKe;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DashboardController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Load mặc định
        public async Task<IActionResult> Index()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var dashboardData = await FetchDashboardDataAsync("thisMonth", null, null);
            if (dashboardData != null)
            {
                ViewBag.CountProduct = dashboardData.TotalProducts;
                ViewBag.CountOrder = dashboardData.TotalOrders;
                ViewBag.CountCategory = dashboardData.TotalCategories;
                ViewBag.CountUser = dashboardData.TotalUsers;

                return View(dashboardData);
            }
            ViewBag.Error = "Không thể tải dữ liệu từ API";
            return View();
        }

        // API cho ajax gọi để vẽ biểu đồ (lọc nhanh hoặc custom date)
        [HttpPost]
        public async Task<IActionResult> GetChartData(string filterType, DateTime? startDate, DateTime? endDate)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var dashboardData = await FetchDashboardDataAsync(filterType, startDate, endDate);
            if (dashboardData != null)
                return Json(dashboardData.ChartData);
            return Json(new { error = "Không thể lấy dữ liệu" });
        }

        // Hàm gọi API thống kê
        private async Task<DashboardStatisticDto?> FetchDashboardDataAsync(string filterType, DateTime? startDate, DateTime? endDate)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var body = new
            {
                filterType = filterType,
                startDate = startDate,
                endDate = endDate
            };
            var response = await client.PostAsJsonAsync("Statistics/dashboard", body);
            if (!response.IsSuccessStatusCode)
                return null;
            var jsonData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DashboardStatisticDto>(jsonData);
        }

        // ✅ Cập nhật GetTopBrands với lọc thời gian
        [HttpPost]
        public async Task<IActionResult> GetTopBrands(string filterType, DateTime? startDate, DateTime? endDate, int top = 3)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");

            var body = new
            {
                filterType = filterType,
                startDate = startDate,
                endDate = endDate
            };

            try
            {
                var response = await client.PostAsJsonAsync($"Statistics/top-brands?top={top}", body);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { error = $"Không thể lấy dữ liệu top thương hiệu: {errorContent}" });
                }

                var json = await response.Content.ReadAsStringAsync();
                var wrapper = JsonConvert.DeserializeObject<ApiResponse<List<TopBrandDto>>>(json);
                return Json(wrapper.Data ?? new List<TopBrandDto>());
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Lỗi kết nối API: {ex.Message}" });
            }
        }

        // ✅ Cập nhật GetTopProducts với lọc thời gian
        [HttpPost]
        public async Task<IActionResult> GetTopProducts(string filterType, DateTime? startDate, DateTime? endDate, int top = 10)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");

            var body = new
            {
                filterType = filterType,
                startDate = startDate,
                endDate = endDate
            };

            try
            {
                var response = await client.PostAsJsonAsync($"Statistics/top-products?top={top}", body);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { error = $"Không thể lấy dữ liệu top sản phẩm: {errorContent}" });
                }

                var jsonData = await response.Content.ReadAsStringAsync();
                var wrapper = JsonConvert.DeserializeObject<ApiResponse<List<TopProductDto>>>(jsonData);
                return Json(wrapper.Data ?? new List<TopProductDto>());
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Lỗi kết nối API: {ex.Message}" });
            }
        }

        // ✅ Lấy thống kê đơn hàng theo trạng thái
        [HttpPost]
        public async Task<IActionResult> GetOrderStatusStatistics(string filterType, DateTime? startDate, DateTime? endDate)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");

            var body = new
            {
                filterType = filterType,
                startDate = startDate,
                endDate = endDate
            };

            try
            {
                var response = await client.PostAsJsonAsync("Statistics/order-status-statistics", body);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { error = $"Không thể lấy dữ liệu thống kê trạng thái đơn hàng: {errorContent}" });
                }

                var jsonData = await response.Content.ReadAsStringAsync();
                var wrapper = JsonConvert.DeserializeObject<ApiResponse<List<OrderStatusStatisticDto>>>(jsonData);
                return Json(wrapper.Data ?? new List<OrderStatusStatisticDto>());
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Lỗi kết nối API: {ex.Message}" });
            }
        }

        public class ApiResponse<T>
        {
            [JsonProperty("statusCode")]
            public int StatusCode { get; set; }

            [JsonProperty("data")]
            public T Data { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }
    }
}