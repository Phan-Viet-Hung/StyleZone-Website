using API.Domain.DTOs;
using API.Domain.Request.ColorRequest;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace MVC.Controllers
{
    [Area("Admin")]
    public class ColorsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ColorsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("color");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không thể tải danh sách màu.";
                return View(new List<ColorDto>());
            }

            var content = await response.Content.ReadAsStringAsync();
            var colors = JsonConvert.DeserializeObject<List<ColorDto>>(content);

            return View(colors);
        }

        public IActionResult Create()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Create(CreateColorRequest request)
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
        { new StringContent(request.Code ?? ""), "Code" }
    };

            var response = await client.PostAsync("color", form);

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = await response.Content.ReadAsStringAsync();

                var allErrors = new List<string>();

                try
                {
                    // TH1: kiểu Dictionary<string, string[]>
                    var errors = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(errorJson);
                    foreach (var kvp in errors)
                    {
                        allErrors.AddRange(kvp.Value);
                    }
                }
                catch
                {
                    try
                    {
                        // TH2: kiểu { "message": "Thông báo lỗi" }
                        var messageObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(errorJson);
                        if (messageObj != null && messageObj.ContainsKey("message"))
                        {
                            allErrors.Add(messageObj["message"]);
                        }
                        else
                        {
                            allErrors.Add(errorJson);
                        }
                    }
                    catch
                    {
                        allErrors.Add(errorJson);
                    }
                }

                TempData["Error"] = string.Join("; ", allErrors); // gán vào popup
                return View(request);
            }



            TempData["Success"] = "Thêm màu thành công.";
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(Guid id)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"color/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không tìm thấy màu.";
                return RedirectToAction("Index");
            }

            var content = await response.Content.ReadAsStringAsync();
            var color = JsonConvert.DeserializeObject<ColorDto>(content);

            var model = new UpdateColorRequest
            {
                Id = color.Id,
                Name = color.Name,
                Code = color.Code
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, UpdateColorRequest request)
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
                { new StringContent(request.Code ?? ""), "Code" },
                { new StringContent(request.Id.ToString()), "Id" }
            };

            var response = await client.PutAsync($"color/{id}", form);

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = await response.Content.ReadAsStringAsync();

                var allErrors = new List<string>();

                try
                {
                    // TH1: kiểu Dictionary<string, string[]>
                    var errors = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(errorJson);
                    foreach (var kvp in errors)
                    {
                        allErrors.AddRange(kvp.Value);
                    }
                }
                catch
                {
                    try
                    {
                        // TH2: kiểu { "message": "Thông báo lỗi" }
                        var messageObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(errorJson);
                        if (messageObj != null && messageObj.ContainsKey("message"))
                        {
                            allErrors.Add(messageObj["message"]);
                        }
                        else
                        {
                            allErrors.Add(errorJson);
                        }
                    }
                    catch
                    {
                        allErrors.Add(errorJson);
                    }
                }

                TempData["Error"] = string.Join("; ", allErrors); // gán vào popup
                return View(request);
            }


            TempData["Success"] = "Cập nhật màu thành công.";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Export()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Gọi API export
            var response = await client.GetAsync("Import/export?entityName=Color");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Export thất bại: " + await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index");
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"ColorExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

    }
}
