using API.DomainCusTomer.DTOs.TrangChu;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Diagnostics;
using System.Security.Policy;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var apiUrlSanPham = "https://localhost:7257/api/TrangChuCustomer/SanPhamTrangChu";
            var apiUrlTinTuc = "https://localhost:7257/api/TrangChuCustomer/TinTucTrangChu";

            var viewModel = new HomepageViewModel();

            try
            {
                viewModel.FeaturedProducts = await client.GetFromJsonAsync<Dictionary<string, List<HomeProductCustomerDto>>>(apiUrlSanPham);

                // G?i thêm Tin t?c khuy?n mãi
                viewModel.Promotions = await client.GetFromJsonAsync<List<HomeProductCustomerDto>>(apiUrlTinTuc);
            }
            catch (Exception ex)
            {
                viewModel.FeaturedProducts = new Dictionary<string, List<HomeProductCustomerDto>>();
                viewModel.Promotions = new List<HomeProductCustomerDto>();
                _logger.LogError(ex, "L?i API: {ApiUrl}", apiUrlSanPham + " ho?c " + apiUrlTinTuc);
            }

            return View(viewModel);
        }

    }
}
