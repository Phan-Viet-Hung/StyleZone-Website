using API.Domain.DTOs;
using API.Domain.Request.CategoryRequest;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;

namespace MVC.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CategoriesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("category");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không thể tải danh sách danh mục.";
                return View(new List<CategoryDto>());
            }

            var content = await response.Content.ReadAsStringAsync();
            var categories = JsonConvert.DeserializeObject<List<CategoryDto>>(content);

            return View(categories);
        }

        public IActionResult Create()
        {var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryRequest request)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (!ModelState.IsValid)
                return View(request);

            var client = _httpClientFactory.CreateClient("ApiClient");

            var form = new MultipartFormDataContent
            {
                { new StringContent(request.Name ?? ""), "Name" },
                { new StringContent(request.Description ?? ""), "Description" }
            };

            var response = await client.PostAsync("category", form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, error);
                TempData["Error"] = "Thêm danh mục thất bại!";
                return View(request);
            }

            TempData["Success"] = "Thêm danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"category/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không tìm thấy danh mục.";
                return RedirectToAction(nameof(Index));
            }

            var content = await response.Content.ReadAsStringAsync();
            var category = JsonConvert.DeserializeObject<CategoryDto>(content);

            var updateRequest = new UpdateCategoryRequest
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            ViewBag.CategoryId = category.Id;
            return View(updateRequest);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, UpdateCategoryRequest request)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (!ModelState.IsValid)
                return View(request);

            var client = _httpClientFactory.CreateClient("ApiClient");

            var form = new MultipartFormDataContent
            {
                { new StringContent(request.Id.ToString()), "Id" },
                { new StringContent(request.Name ?? ""), "Name" },
                { new StringContent(request.Description ?? ""), "Description" }
            };

            var response = await client.PutAsync($"category/{id}", form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, error);
                TempData["Error"] = "Cập nhật danh mục thất bại!";
                return View(request);
            }

            TempData["Success"] = "Cập nhật danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Export()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Gọi API export
            var response = await client.GetAsync("Import/export?entityName=Category");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Export thất bại: " + await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index");
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"CategoryExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}
