using API.Domain.DTOs;
using API.Domain.Request.BrandRequest;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace MVC.Controllers
{
    [Area("Admin")]
    public class BrandsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BrandsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("brand");

            if (!response.IsSuccessStatusCode)
                return View(new List<BrandDto>());

            var content = await response.Content.ReadAsStringAsync();
            var brands = JsonConvert.DeserializeObject<List<BrandDto>>(content);

            return View(brands);
        }


        public IActionResult Create()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBrandRequest request)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (!ModelState.IsValid)
                return View(request);

            var client = _httpClientFactory.CreateClient("ApiClient");

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(request.Code ?? ""), "Code");
            form.Add(new StringContent(request.Name ?? ""), "Name");

            var response = await client.PostAsync("brand", form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, error);
                TempData["Error"] = "Thêm thương hiệu thất bại!";
                return View(request);
            }

            TempData["Success"] = "Thêm thương hiệu thành công!";
            return RedirectToAction("Index");
        }

        


        public async Task<IActionResult> Edit(Guid id)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"brand/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var content = await response.Content.ReadAsStringAsync();
            var brand = JsonConvert.DeserializeObject<BrandDto>(content);

            var updateRequest = new UpdateBrandRequest
            {
                Code = brand.Code,
                Name = brand.Name
            };

            ViewBag.BrandId = brand.Id;
            return View(updateRequest);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, UpdateBrandRequest request)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (!ModelState.IsValid)
                return View(request);

            var client = _httpClientFactory.CreateClient("ApiClient");

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(request.Code ?? ""), "Code");
            form.Add(new StringContent(request.Name ?? ""), "Name");

            var response = await client.PutAsync($"brand/{id}", form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, error);
                TempData["Error"] = "Cập nhật thương hiệu thất bại!";
                return View(request);
            }

            TempData["Success"] = "Cập nhật thương hiệu thành công!";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Export()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Gọi API export
            var response = await client.GetAsync("Import/export?entityName=Brand");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Export thất bại: " + await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index");
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"BrandExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}
