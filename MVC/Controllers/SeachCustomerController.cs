using API.DomainCusTomer.DTOs.SeachCustomer;
using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers
{
    public class SeachCustomerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public SeachCustomerController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> Index(string? keyword)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "https://localhost:7257/api/SeachCustomer";

            if (!string.IsNullOrWhiteSpace(keyword))
                apiUrl += $"?keyword=" + keyword;

            var response = await client.GetAsync(apiUrl);

            var products = new List<ProductSearchResultDto>();

            if (response.IsSuccessStatusCode)
            {
                products = await response.Content.ReadFromJsonAsync<List<ProductSearchResultDto>>();
            }

            return View(products);
        }
    }
}
