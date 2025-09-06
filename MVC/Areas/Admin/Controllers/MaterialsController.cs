using API.Domain.DTOs;
using API.Domain.Request.MaterialRequest;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MaterialsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MaterialsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "MVCAuth");

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("material");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Lỗi khi tải danh sách chất liệu: " + await response.Content.ReadAsStringAsync();
                return View(new List<MaterialDto>());
            }

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<MaterialDto>>(content) ?? new List<MaterialDto>();
            return View(data);
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
        public async Task<IActionResult> Create(CreateMaterialRequest request)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "MVCAuth");

            if (!ModelState.IsValid)
                return View(request);

            // Validate các trường
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                TempData["Error"] = "Tên chất liệu không được để trống.";
                return View(request);
            }
            if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 500)
            {
                TempData["Error"] = "Mô tả không được vượt quá 500 ký tự.";
                return View(request);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");

            var form = new MultipartFormDataContent
            {
                { new StringContent(request.Name ?? ""), "Name" },
                { new StringContent(request.Description ?? ""), "Description" }
            };

            var response = await client.PostAsync("material", form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = "Thêm chất liệu thất bại: " + ExtractMessage(error);
                return View(request);
            }

            TempData["Success"] = "Thêm chất liệu thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "MVCAuth");

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"material/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không tìm thấy chất liệu: " + await response.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(Index));
            }

            var content = await response.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<MaterialDto>(content);
            if (dto == null)
                return RedirectToAction(nameof(Index));

            var model = new UpdateMaterialRequest
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateMaterialRequest request)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "MVCAuth");

            if (!ModelState.IsValid)
                return View(request);

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                TempData["Error"] = "Tên chất liệu không được để trống.";
                return View(request);
            }

            if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 500)
            {
                TempData["Error"] = "Mô tả không được vượt quá 500 ký tự.";
                return View(request);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");

            var form = new MultipartFormDataContent
            {
                { new StringContent(id.ToString()), "Id" },
                { new StringContent(request.Name ?? ""), "Name" },
                { new StringContent(request.Description ?? ""), "Description" }
            };

            var response = await client.PutAsync($"material/{id}", form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = "Cập nhật chất liệu thất bại: " + ExtractMessage(error);
                return View(request);
            }

            TempData["Success"] = "Cập nhật chất liệu thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Export()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "MVCAuth");

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("Import/export?entityName=Material");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Export thất bại: " + await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index");
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"MaterialExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

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
