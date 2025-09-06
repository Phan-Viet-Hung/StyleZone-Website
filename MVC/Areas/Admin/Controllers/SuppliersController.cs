using API.Domain.DTOs;
using API.Domain.Request.SupplierRequest;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace MVC.Controllers
{
    [Area("Admin")]
    public class SuppliersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SuppliersController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Admin/Supplier
        public async Task<IActionResult> Index()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("supplier");

            if (!response.IsSuccessStatusCode)
                return View(new List<SupplierDto>());

            var content = await response.Content.ReadAsStringAsync();
            var suppliers = JsonConvert.DeserializeObject<List<SupplierDto>>(content);

            return View(suppliers);
        }

        // GET: /Admin/Supplier/Create
        public IActionResult Create()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSupplierRequest request)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (!ModelState.IsValid)
                return View(request);

            request.Name = request.Name?.ToLower().Trim(); // chuẩn hóa tên

            var client = _httpClientFactory.CreateClient("ApiClient");
            var form = new MultipartFormDataContent
    {
        { new StringContent(request.Name ?? ""), "Name" },
        { new StringContent(request.Contact ?? ""), "Contact" },
        { new StringContent(request.Email ?? ""), "Email" },
        { new StringContent(request.Address ?? ""), "Address" }
    };

            var response = await client.PostAsync("supplier", form);



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
                        // TH2: kiểu { "message": "Email đã tồn tại." }
                        var messageObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(errorJson);
                        if (messageObj != null && messageObj.ContainsKey("message"))
                        {
                            allErrors.Add(messageObj["message"]);
                        }
                        else
                        {
                            allErrors.Add(errorJson); // fallback
                        }
                    }
                    catch
                    {
                        allErrors.Add(errorJson); // fallback nếu cả 2 TH đều fail
                    }
                }

                TempData["Error"] = string.Join("; ", allErrors);
                return View(request);
            }



            TempData["Success"] = "Thêm nhà cung cấp thành công!";
            return RedirectToAction("Index");
        }


        // GET: /Admin/Supplier/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"supplier/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var content = await response.Content.ReadAsStringAsync();
            var supplier = JsonConvert.DeserializeObject<SupplierDto>(content);

            var updateRequest = new UpdateSupplierRequest
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Contact = supplier.Contact,
                Email = supplier.Email,
                Address = supplier.Address
            };


            return View(updateRequest);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, UpdateSupplierRequest request)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (!ModelState.IsValid)
                return View(request);

            request.Name = request.Name?.ToLower().Trim(); // chuẩn hóa tên

            var client = _httpClientFactory.CreateClient("ApiClient");
            var form = new MultipartFormDataContent
    {
        { new StringContent(request.Id.ToString()), "Id" },
        { new StringContent(request.Name ?? ""), "Name" },
        { new StringContent(request.Contact ?? ""), "Contact" },
        { new StringContent(request.Email ?? ""), "Email" },
        { new StringContent(request.Address ?? ""), "Address" }
    };

            var response = await client.PutAsync($"supplier/{id}", form);

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
                        // TH2: kiểu { "message": "Email đã tồn tại." }
                        var messageObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(errorJson);
                        if (messageObj != null && messageObj.ContainsKey("message"))
                        {
                            allErrors.Add(messageObj["message"]);
                        }
                        else
                        {
                            allErrors.Add(errorJson); // fallback
                        }
                    }
                    catch
                    {
                        allErrors.Add(errorJson); // fallback nếu cả 2 TH đều fail
                    }
                }

                TempData["Error"] = string.Join("; ", allErrors);
                return View(request);
            }
            TempData["Success"] = "Cập nhật nhà cung cấp thành công!";
            return RedirectToAction("Index");
        }


    }
}
