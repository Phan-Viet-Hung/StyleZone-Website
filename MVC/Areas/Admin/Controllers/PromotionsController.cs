using API.Domain.DTOs;
using API.Domain.Request.PromotionRequest;
using API.Domain.Request.VoucherRequest;
using DAL_Empty.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PromotionsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PromotionsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("Promotion");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không thể tải danh sách chương trình khuyến mãi.";
                return View(new List<PromotionDto>());
            }
            var list = await response.Content.ReadFromJsonAsync<List<PromotionDto>>();
            return View(list);
        }

        public IActionResult Create()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var model = new CreatePromotionRequest
            {
                StartDate = DateTime.Now, // Hoặc DateTime.Today
                EndDate = DateTime.Now.AddDays(7) // Mặc định 7 ngày sau
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePromotionRequest request)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
                return View(request);
            }

            // ✅ Check nếu người dùng không chọn sản phẩm
            if (request.ProductDetailIds == null || !request.ProductDetailIds.Any())
            {
                TempData["Error"] = "Bạn phải chọn ít nhất một sản phẩm để tạo chương trình khuyến mãi.";
                return View(request);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var content = new MultipartFormDataContent();

            content.Add(new StringContent(request.Name ?? ""), nameof(request.Name));
            content.Add(new StringContent(request.Description ?? ""), nameof(request.Description));
            content.Add(new StringContent(request.DiscountType.ToString()), nameof(request.DiscountType));
            content.Add(new StringContent(request.DiscountValue.ToString()), nameof(request.DiscountValue));
            content.Add(new StringContent(request.Status.ToString()), nameof(request.Status));
            content.Add(new StringContent(request.StartDate.ToString("o")), nameof(request.StartDate));
            content.Add(new StringContent(request.EndDate.ToString("o")), nameof(request.EndDate));

            foreach (var id in request.ProductDetailIds)
            {
                content.Add(new StringContent(id.ToString()), "ProductDetailIds");
            }

            if (!string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                content.Add(new StringContent(request.ImageUrl), nameof(request.ImageUrl));
            }
            else if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var stream = request.ImageFile.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ImageFile.ContentType);
                content.Add(fileContent, nameof(request.ImageFile), request.ImageFile.FileName);
            }
            else
            {
                TempData["Error"] = "Bạn phải tải ảnh lên.";
                return View(request);
            }

            var response = await client.PostAsync("Promotion", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Tạo chương trình khuyến mãi thành công!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                try
                {
                    var errors = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(error);
                    foreach (var kvp in errors)
                    {
                        ModelState.AddModelError(kvp.Key, string.Join(", ", kvp.Value));
                    }
                }
                catch
                {
                    TempData["Error"] = ExtractErrorMessage(error);
                }
                return View(request);
            }
        }



        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"Promotion/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không thể tải dữ liệu chương trình khuyến mãi.";
                return RedirectToAction(nameof(Index));
            }

            var dto = await response.Content.ReadFromJsonAsync<PromotionDto>();
            if (dto == null)
            {
                TempData["Error"] = "Chương trình khuyến mãi không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            // Gọi API lấy danh sách tất cả sản phẩm
            var productResponse = await client.GetAsync("product-details");
            var allProducts = new List<ProductDetailDto>();
            if (productResponse.IsSuccessStatusCode)
            {
                allProducts = await productResponse.Content.ReadFromJsonAsync<List<ProductDetailDto>>() ?? new();
            }

            ViewBag.AllProducts = allProducts.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name,
                Selected = dto.ProductDetailIds?.Contains(p.Id) ?? false
            }).ToList();

            var model = new UpdatePromotionRequest
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                DiscountType = Enum.Parse<DiscountType>(dto.DiscountType),
                DiscountValue = dto.DiscountValue,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = Enum.Parse<VoucherStatus>(dto.Status),
                ImageUrl = dto.ImageUrl,
                ProductDetailIds = dto.ProductDetailIds
            };
            return View(model);
        }


        [HttpPost] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdatePromotionRequest request)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}")
                    .ToList();

                TempData["Error"] = "Dữ liệu cập nhật không hợp lệ:\n" + string.Join("\n", errors);
                return View(request);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var content = new MultipartFormDataContent();

            content.Add(new StringContent(request.Id.ToString()), nameof(request.Id));
            content.Add(new StringContent(request.Name ?? ""), nameof(request.Name));
            content.Add(new StringContent(request.Description ?? ""), nameof(request.Description));
            content.Add(new StringContent(request.DiscountType.ToString()), nameof(request.DiscountType));
            content.Add(new StringContent(request.DiscountValue.ToString()), nameof(request.DiscountValue));
            content.Add(new StringContent(request.Status.ToString()), nameof(request.Status));
            content.Add(new StringContent(request.StartDate.ToString("o")), nameof(request.StartDate));
            content.Add(new StringContent(request.EndDate.ToString("o")), nameof(request.EndDate));
            if (request.ProductDetailIds != null && request.ProductDetailIds.Any())
            {
                foreach (var id in request.ProductDetailIds)
                {
                    content.Add(new StringContent(id.ToString()), "ProductDetailIds");
                }
            }
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var stream = request.ImageFile.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ImageFile.ContentType);
                content.Add(fileContent, nameof(request.ImageFile), request.ImageFile.FileName);
            }
            else if (!string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                content.Add(new StringContent(request.ImageUrl), nameof(request.ImageUrl));
            }
            else
            {
                TempData["Error"] = "Bạn phải tải ảnh lên.";
                return View(request);
            }

            var response = await client.PutAsync("Promotion", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Cập nhật chương trình khuyến mãi thành công!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                try
                {
                    var errors = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(error);
                    foreach (var kvp in errors)
                    {
                        ModelState.AddModelError(kvp.Key, string.Join(", ", kvp.Value));
                    }

                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật:\n" +
                                        string.Join("\n", errors.SelectMany(x => x.Value.Select(v => $"{x.Key}: {v}")));
                }
                catch
                {
                    TempData["Error"] = ExtractErrorMessage(error);
                }

                return View(request);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"Promotion/detail/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không thể tải chi tiết chương trình khuyến mãi.";
                return RedirectToAction(nameof(Index));
            }

            var detail = await response.Content.ReadFromJsonAsync<PromotionDetailDto>();
            if (detail == null)
            {
                TempData["Error"] = "Chương trình khuyến mãi không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            var productIds = detail.PromotionProducts.Select(p => p.ProductDetailId).ToList();

            var idsParam = string.Join("&ids=", productIds);

            // SỬA ĐƯỜNG DẪN GỌI ĐÚNG ROUTE API
            var productsResponse = await client.GetAsync($"product-details/by-ids?ids={idsParam}");

            if (productsResponse.IsSuccessStatusCode)
            {
                var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDetailDto>>();

                if (products != null)
                {
                    var map = products.ToDictionary(
                        x => x.Id.ToString(),
                        x => new { x.Name, x.MainImageUrl }
                    );
                    ViewBag.ProductMap = map;
                }
            }

            return View(detail);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(Guid id, string newStatus)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (string.IsNullOrEmpty(newStatus))
            {
                TempData["Error"] = "Trạng thái không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var client = _httpClientFactory.CreateClient("ApiClient");

            var data = new Dictionary<string, string>
            {
                { "Id", id.ToString() },
                { "Status", newStatus }
            };

            var content = new FormUrlEncodedContent(data);

            var response = await client.PutAsync("Promotion/change-status", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Cập nhật trạng thái thành công!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = ExtractErrorMessage(error);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkChangeStatus(List<Guid> ids, string status)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (ids == null || !ids.Any() || string.IsNullOrEmpty(status))
            {
                TempData["Error"] = "Danh sách ID hoặc trạng thái không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var client = _httpClientFactory.CreateClient("ApiClient");

            var keyValues = new List<KeyValuePair<string, string>>();
            foreach (var id in ids)
                keyValues.Add(new KeyValuePair<string, string>("Ids", id.ToString()));
            keyValues.Add(new KeyValuePair<string, string>("Status", status));

            var content = new FormUrlEncodedContent(keyValues);

            var response = await client.PutAsync("promotion/change-status-bulk", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Cập nhật trạng thái hàng loạt thành công!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = ExtractErrorMessage(error);
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Export()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Gọi API export
            var response = await client.GetAsync("Import/export?entityName=Promotion");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Export thất bại: " + await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index");
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"PromotionExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        private string ExtractErrorMessage(string json)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<ErrorResponse>(json);
                if (!string.IsNullOrEmpty(obj?.message))
                    return obj.message;
                return "Có lỗi xảy ra khi xử lý dữ liệu.";
            }
            catch
            {
                return json.Length < 200 ? json : "Có lỗi không xác định.";
            }
        }

        private class ErrorResponse
        {
            public string message { get; set; } = "";
        }
    }
}
