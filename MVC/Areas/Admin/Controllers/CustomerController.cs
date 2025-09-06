using API.Domain.DTOs;
using API.Domain.Request.CustomerRequest;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CustomerController : Controller
    {
        private readonly HttpClient _httpClient;

        public CustomerController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        // GET: Danh sách khách hàng
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var response = await _httpClient.GetAsync("customer");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không thể tải danh sách khách hàng.";
                return View(new List<CustomerDto>());
            }

            var content = await response.Content.ReadAsStringAsync();
            var customers = JsonConvert.DeserializeObject<List<CustomerDto>>(content) ?? new List<CustomerDto>();
            return View(customers);
        }

        // GET: Chi tiết khách hàng
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var response = await _httpClient.GetAsync($"customer/{id}");
            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response);
                return RedirectToAction(nameof(Index));
            }

            var content = await response.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<CustomerDto>(content);
            return View(customer);
        }

        // PATCH: Cập nhật trạng thái 1 khách hàng
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid id, string status)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (id == Guid.Empty || string.IsNullOrWhiteSpace(status))
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var response = await _httpClient.PatchAsync($"customer/{id}/status?status={status}", null);

            TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                response.IsSuccessStatusCode
                ? "Cập nhật trạng thái thành công."
                : "Cập nhật trạng thái thất bại.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatusBulk([FromBody] UpdateStatusBulkRequest request)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (request == null || request.Ids == null || !request.Ids.Any() || string.IsNullOrWhiteSpace(request.Status))
                return Json(new { success = false });

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("customer/status/bulk", content);
            return Json(new { success = response.IsSuccessStatusCode });
        }


        private void HandleErrorResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
                TempData["Error"] = "Không tìm thấy khách hàng.";
            else
                TempData["Error"] = "Đã xảy ra lỗi khi truy vấn khách hàng.";
        }
        public async Task<IActionResult> Export()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");

            // Gọi API export
            var response = await _httpClient.GetAsync("Import/export?entityName=Customer");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Export thất bại: " + await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index");
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"CustomerExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}
