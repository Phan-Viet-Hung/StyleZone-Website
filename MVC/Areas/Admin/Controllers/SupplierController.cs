using DAL_Empty.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SupplierController : Controller
    {
        private readonly HttpClient _httpClient;
        public SupplierController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7257/"); // địa chỉ API
        }
        public async Task<IActionResult> Index()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var res = await _httpClient.GetAsync("api/SupplierApi");
            var json = await res.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<Supplier>>(json);
            return View(data);
        }
        public IActionResult Create()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            return View();
        }

        [HttpPost]
      
        public async Task<IActionResult> Create([Bind("Id,Code,Name,Contact,Email,Address")] Supplier supplier)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (!ModelState.IsValid)
            {
                return View(supplier); // hiển thị lỗi nhập liệu
            }
           
            supplier.Id = Guid.NewGuid();
            //var json = JsonConvert.SerializeObject(supplier);
            var json = System.Text.Json.JsonSerializer.Serialize(supplier);
           
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _httpClient.PostAsync("https://localhost:7257/api/SupplierApi", content);
            if (res.IsSuccessStatusCode)
                return RedirectToAction("Index");

            return View(supplier);
            //using (var httpClient = new HttpClient())
            //{
            //    StringContent content = new StringContent(JsonConvert.SerializeObject(supplier), Encoding.UTF8, "application/json");
            //    using 
            //        (var response = await httpClient.PostAsync("https://localhost:7257/api/SupplierApi", content))
            //    {
            //        string api = await response.Content.ReadAsStringAsync();
            //        supplier = JsonConvert.DeserializeObject<Supplier>(api);
            //    }
            //    return RedirectToAction("Index");
            //}

        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var res = await _httpClient.GetAsync($"api/SupplierApi/{id}");
            var json = await res.Content.ReadAsStringAsync();
            var supplier = JsonConvert.DeserializeObject<Supplier>(json);
            return View(supplier);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, Supplier supplier)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (id != supplier.Id)
                return BadRequest("ID không khớp");

            var json = JsonConvert.SerializeObject(supplier);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _httpClient.PutAsync($"api/SupplierApi/{id}", content);
            if (res.IsSuccessStatusCode)
                return RedirectToAction("Index");

            return View(supplier);
        }
        public async Task<IActionResult> Export()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");

            // Gọi API export
            var response = await _httpClient.GetAsync("Import/export?entityName=Supplier");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Export thất bại: " + await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index");
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"SupplierExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}
