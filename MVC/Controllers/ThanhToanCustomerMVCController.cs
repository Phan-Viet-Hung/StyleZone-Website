using API.DomainCusTomer.DTOs.ThanhToanCustomer;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace MVC.Controllers
{
    public class ThanhToanCustomerMVCController : Controller
    {
        private readonly HttpClient _httpClient;

        public ThanhToanCustomerMVCController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7257/api/"); 
        }

        //[HttpGet]
        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpPost]
        public async Task<IActionResult> CreateGuestOrder(OrderGuestDto request)
        {
            if (!ModelState.IsValid)
                return View("IndexMuaNgay", request); // Load lại form thanh toán với dữ liệu

            var jsonData = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("ThanhToanCustomer/create-guest-order", content);

            if (response.IsSuccessStatusCode)
            {
                var orderId = await response.Content.ReadFromJsonAsync<Guid>();

                // Lưu ID đơn hàng vào Session
                HttpContext.Session.SetString("LastOrderId", orderId.ToString());
                TempData["SuccessMessage"] = "Đặt hàng thành công!";
                return RedirectToAction("Index", "Home");  // chuyển tới trang thành công
            }
            else
            {
                var errorText = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"{errorText}");
                return View("IndexMuaNgay", request);
            }
        }


        //[HttpGet]
        //public IActionResult OrderSuccess()
        //{
        //    return View();
        //}
    }
}
