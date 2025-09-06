using API.Domain.DTOs;
using API.Domain.Request.OriginRequest;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OriginsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OriginsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "MVCAuth");

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("origin");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Lỗi khi tải danh sách xuất xứ: " + await response.Content.ReadAsStringAsync();
                return View(new List<OriginDto>());
            }

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<OriginDto>>(content) ?? new List<OriginDto>();
            return View(data);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "MVCAuth");

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"origin/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không tìm thấy xuất xứ: " + await response.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(Index));
            }

            var content = await response.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<OriginDto>(content);

            if (dto == null)
                return RedirectToAction(nameof(Index));

            var model = new UpdateOriginRequest
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateOriginRequest request)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "MVCAuth");

            if (!ModelState.IsValid)
                return View(request);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync("origin", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = "Sửa thất bại: " + ExtractMessage(error);
                return View(request);
            }


            TempData["Success"] = "Cập nhật thành công.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "MVCAuth");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOriginRequest request)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "MVCAuth");

            if (!ModelState.IsValid)
                return View(request);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("origin", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = "Tạo thất bại: " + ExtractMessage(error);
                return View(request);
            }


            TempData["Success"] = "Tạo thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Export()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "MVCAuth");

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("Import/export?entityName=Origin");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Export thất bại: " + await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index");
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"OriginExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        private string ExtractMessage(string json)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<ErrorResponse>(json);
                return obj?.message ?? "Có lỗi xảy ra.";
            }
            catch
            {
                return "Có lỗi xảy ra.";
            }
        }

        private class ErrorResponse
        {
            public string message { get; set; }
        }

    }
}
