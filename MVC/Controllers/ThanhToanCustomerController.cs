using API.DomainCusTomer.DTOs.CartICustomer;
using API.DomainCusTomer.DTOs.MuangayCustomer;
using API.DomainCusTomer.DTOs.ThanhToanCustomer;
using API.DomainCusTomer.Request.GHN;
using API.DomainCusTomer.Request.MuaNgay;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MVC.Controllers
{
    public class ThanhToanCustomerController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string CookieCartKey = "CustomerCart";
        public ThanhToanCustomerController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7257/api/");
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [HttpPost]
        public async Task<IActionResult> AddMuaNgay(MuaNgayCustomerRequest request)
        {
            string username = HttpContext.Request.Cookies["UserName"] ?? HttpContext.Request.Cookies["LoginMethod"];

            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "Home");
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("CartCustomer/addmua-ngay", content);
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Số lượng không đủ.");
            }
            return RedirectToAction("IndexMuaNgay", "ThanhToanCustomer");
        }
        // ========== TẠO ĐƠN HÀNG ==========

        [HttpGet]
        public async Task<IActionResult> IndexMuaNgay()
        {
            //string username = HttpContext.Request.Cookies["UserName"] ?? HttpContext.Request.Cookies["LoginMethod"];

            //if (!string.IsNullOrEmpty(username))
            //    return RedirectToAction("Index", "Home");
            var response = await _httpClient.GetAsync("CartCustomer/currentmua-ngay");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không có sản phẩm mua ngay.";
                return RedirectToAction("Index", "Home");
            }
            var item = await response.Content.ReadFromJsonAsync<MuangaycustomerDto>();
            return View(item);
        }
        public async Task<decimal> GetShippingFeeAsync(ShippingFeeRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("shipping/calculate-fee", request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("total_fee").GetDecimal();
        }
        public async Task<IActionResult> IndexThanhToan()
        {
            string username = HttpContext.Request.Cookies["UserName"] ?? HttpContext.Request.Cookies["LoginMethod"];

            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "Home");
            if (Request.Cookies.TryGetValue(CookieCartKey, out var json))
            {
                var cartItems = JsonConvert.DeserializeObject<List<CartCustomerDto>>(json);
                return View(cartItems);
            }
            return View(new List<CartCustomerDto>());
        }


    }
}
