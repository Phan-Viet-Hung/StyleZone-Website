using API.Domain.DTOs;
using API.Domain.Request.ProductDetailRequest;
using API.Domain.Request.VoucherRequest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductDetailsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductDetailsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private async Task LoadDropdownsAsync()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var colors = await client.GetFromJsonAsync<List<ColorDto>>("color");
            var sizes = await client.GetFromJsonAsync<List<SizeDto>>("size");
            var materials = await client.GetFromJsonAsync<List<MaterialDto>>("material");
            var origins = await client.GetFromJsonAsync<List<OriginDto>>("origin");
            var suppliers = await client.GetFromJsonAsync<List<SupplierDto>>("supplier");
            var products = await client.GetFromJsonAsync<List<ProductDto>>("product");
            var categories = await client.GetFromJsonAsync<List<CategoryDto>>("category");
            var brands = await client.GetFromJsonAsync<List<BrandDto>>("brand");

            ViewBag.Colors = colors?.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            ViewBag.Sizes = sizes?.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
            ViewBag.Materials = materials?.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }).ToList();
            ViewBag.Origins = origins?.Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name }).ToList();
            ViewBag.Suppliers = suppliers?.Select(su => new SelectListItem { Value = su.Id.ToString(), Text = su.Name }).ToList();
            ViewBag.Products = products?.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
            ViewBag.Categories = categories?.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            ViewBag.Brands = brands?.Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name }).ToList();
        }

        // GET: Admin/ProductDetails/Create
        public async Task<IActionResult> Create(Guid? productId)
        {
            await LoadDropdownsAsync();
            var model = new CreateProductDetailRequest
            {
                ProductId = productId ?? Guid.Empty
            };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDetailRequest request)
        {
            if (!ModelState.IsValid)
            {
                // load dropdown, trả về view lỗi
                await LoadDropdownsAsync();
                return View(request);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");

            // Bước 1: Tạo product detail
            var form = new MultipartFormDataContent
    {
        { new StringContent(request.Price.ToString()), "Price" },
        { new StringContent(request.Code.ToString()), "Code" },
        { new StringContent(request.Quantity.ToString()), "Quantity" },
        { new StringContent(request.ProductId.ToString()), "ProductId" },
        { new StringContent(request.Status.ToString()), "Status" }
    };
            if (request.ColorId != null) form.Add(new StringContent(request.ColorId.ToString()), "ColorId");
            if (request.SizeId != null) form.Add(new StringContent(request.SizeId.ToString()), "SizeId");
            if (request.MaterialId != null) form.Add(new StringContent(request.MaterialId.ToString()), "MaterialId");
            if (request.OriginId != null) form.Add(new StringContent(request.OriginId.ToString()), "OriginId");
            if (request.SupplierId != null) form.Add(new StringContent(request.SupplierId.ToString()), "SupplierId");

            var createResponse = await client.PostAsync("product-details", form);
            if (!createResponse.IsSuccessStatusCode)
            {
                var error = await createResponse.Content.ReadAsStringAsync();
                TempData["Error"] = ExtractMessage(error);
                await LoadDropdownsAsync();
                return View(request);
            }

            var createdProductDetail = await createResponse.Content.ReadFromJsonAsync<ProductDetailDto>();
            if (createdProductDetail == null)
            {
                TempData["Error"] = "Không lấy được ID sản phẩm chi tiết sau khi tạo";
                await LoadDropdownsAsync();
                return View(request);
            }

            // Bước 2: Upload ảnh nếu có
            if (request.ImageFiles != null && request.ImageFiles.Any())
            {
                var imageForm = new MultipartFormDataContent();
                imageForm.Add(new StringContent(createdProductDetail.Id.ToString()), "ProductDetailId");

                // Dùng giá trị thực của MainImageIndex gửi lên API
                imageForm.Add(new StringContent((request.MainImageIndex ?? 0).ToString()), "MainImageIndex");

                foreach (var file in request.ImageFiles)
                {
                    var fileContent = new StreamContent(file.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    imageForm.Add(fileContent, "Files", file.FileName);
                }

                var uploadResponse = await client.PostAsync("image/upload-multiple", imageForm);
                if (!uploadResponse.IsSuccessStatusCode)
                {
                    var error = await uploadResponse.Content.ReadAsStringAsync();
                    TempData["Error"] = "Tạo chi tiết thành công, nhưng upload ảnh thất bại: " + ExtractMessage(error);
                    return RedirectToAction("Details", "Products", new { area = "Admin", id = request.ProductId });
                }
            }


            TempData["Success"] = "Tạo chi tiết sản phẩm và upload ảnh thành công!";
            return RedirectToAction("Details", "Products", new { area = "Admin", id = request.ProductId });
        }





        // GET: Admin/ProductDetails/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"product-details/{id}");

            if (!response.IsSuccessStatusCode) return NotFound();

            var content = await response.Content.ReadAsStringAsync();
            var detail = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductDetailDto>(content);

            if (detail == null)
                return NotFound();

            var images = detail.Images;
            if (images == null || images.Count == 0)
            {
                images = detail.ImageUrls?.Select(url => new ImageDto { Url = url, IsMainImage = false }).ToList() ?? new List<ImageDto>();
            }

            var request = new UpdateProductDetailRequest
            {
                Id = detail.Id,
                Code = detail.Code,
                Price = detail.Price,
                Quantity = detail.Quantity,
                ProductId = detail.ProductId,
                ColorId = detail.ColorId,
                SizeId = detail.SizeId,
                MaterialId = detail.MaterialId,
                OriginId = detail.OriginId,
                SupplierId = detail.SupplierId,

                // Truyền danh sách ảnh cũ (dạng ImageDto) xuống View
                ExistingImages = images,

                // Chọn ảnh chính hiện tại (index trong danh sách ảnh cũ)
                MainImageIndex = images.FindIndex(i => i.IsMainImage)
            };

            await LoadDropdownsAsync();
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, UpdateProductDetailRequest request)
        {
            if (id != request.Id || !ModelState.IsValid)
            {
                TempData["Error"] = id != request.Id
                    ? "ID không khớp với dữ liệu gửi lên."
                    : GetModelStateErrors();

                await LoadDropdownsAsync();
                return View(request);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");

            // 1. Update product detail info
            var form = new MultipartFormDataContent
    {
        { new StringContent(request.Id.ToString()), "Id" },
        { new StringContent(request.Code ?? ""), "Code" },
        { new StringContent(request.Price.ToString()), "Price" },
        { new StringContent(request.Quantity.ToString()), "Quantity" },
        { new StringContent(request.ProductId.ToString()), "ProductId" },
        { new StringContent(request.Status.ToString()), "Status" }
    };

            if (request.ColorId != null) form.Add(new StringContent(request.ColorId.ToString()), "ColorId");
            if (request.SizeId != null) form.Add(new StringContent(request.SizeId.ToString()), "SizeId");
            if (request.MaterialId != null) form.Add(new StringContent(request.MaterialId.ToString()), "MaterialId");
            if (request.OriginId != null) form.Add(new StringContent(request.OriginId.ToString()), "OriginId");
            if (request.SupplierId != null) form.Add(new StringContent(request.SupplierId.ToString()), "SupplierId");

            var response = await client.PutAsync($"product-details/{id}", form);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"API: {ExtractMessage(error)}";
                await LoadDropdownsAsync();
                return View(request);
            }

            // 2. Handle images
            if (request.ImageFiles != null && request.ImageFiles.Any())
            {
                // --- Case 1: Upload new images ---
                int mainIndex = request.MainImageIndex ?? 0;
                if (mainIndex < 0 || mainIndex >= request.ImageFiles.Count())
                    mainIndex = 0;

                // Xóa ảnh cũ
                var deleteResponse = await client.DeleteAsync($"image/product-detail/{request.Id}");
                if (!deleteResponse.IsSuccessStatusCode)
                {
                    var err = await deleteResponse.Content.ReadAsStringAsync();
                    TempData["Error"] = "Cập nhật chi tiết thành công, nhưng xóa ảnh cũ thất bại: " + ExtractMessage(err);
                    return RedirectToAction("Details", "Products", new { area = "Admin", id = request.ProductId });
                }

                // Upload ảnh mới
                var imageForm = new MultipartFormDataContent
        {
            { new StringContent(request.Id.ToString()), "ProductDetailId" },
            { new StringContent(mainIndex.ToString()), "MainImageIndex" }
        };

                foreach (var file in request.ImageFiles)
                {
                    var fileContent = new StreamContent(file.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    imageForm.Add(fileContent, "Files", file.FileName);
                }

                var uploadResponse = await client.PostAsync("image/upload-multiple", imageForm);
                if (!uploadResponse.IsSuccessStatusCode)
                {
                    var error = await uploadResponse.Content.ReadAsStringAsync();
                    TempData["Error"] = "Cập nhật chi tiết thành công, nhưng upload ảnh thất bại: " + ExtractMessage(error);
                    return RedirectToAction("Details", "Products", new { area = "Admin", id = request.ProductId });
                }
            }
            else if (request.ExistingImages != null && request.ExistingImages.Count > 0)
            {
                // --- Case 2: No new images -> Set main image from existing ones ---
                int mainIndex = request.MainImageIndex ?? 0;
                if (mainIndex < 0 || mainIndex >= request.ExistingImages.Count)
                    mainIndex = 0;

                var setMainRequest = new
                {
                    ImageId = request.ExistingImages[mainIndex].Id,
                    ProductDetailId = request.Id
                };

                var setMainResponse = await client.PutAsJsonAsync("image/set-main", setMainRequest);
                if (!setMainResponse.IsSuccessStatusCode)
                {
                    var error = await setMainResponse.Content.ReadAsStringAsync();
                    TempData["Error"] = "Cập nhật chi tiết thành công, nhưng đặt ảnh chính thất bại: " + ExtractMessage(error);
                    return RedirectToAction("Details", "Products", new { area = "Admin", id = request.ProductId });
                }
            }

            TempData["Success"] = "Cập nhật chi tiết sản phẩm thành công!";
            return RedirectToAction("Details", "Products", new { area = "Admin", id = request.ProductId });
        }

        // GET: Admin/ProductDetails/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"product-details/{id}");

            if (!response.IsSuccessStatusCode) return NotFound();

            var content = await response.Content.ReadAsStringAsync();
            var detail = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductDetailDto>(content);

            return View(detail);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(ChangeStatusRequest request,Guid productId)
        {
            if (request == null || request.Id == Guid.Empty || string.IsNullOrWhiteSpace(request.Status))
            {
                TempData["Error"] = "Dữ liệu trạng thái không hợp lệ.";
                return RedirectToAction("Index", "Products", new { area = "Admin" });
            }

            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync("product-details/change-status", content);


                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Cập nhật trạng thái thành công!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"{ExtractMessage(error)}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi hệ thống: {ex.Message}";
            }

            // Luôn redirect về trang Index của Products
            return RedirectToAction("Details", "Products", new { area = "Admin", id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkChangeStatus(BulkStatusChangeRequest request)
        {
            if (request == null || request.Ids == null || !request.Ids.Any() || string.IsNullOrWhiteSpace(request.Status))
            {
                TempData["Error"] = "Dữ liệu thay đổi trạng thái không hợp lệ.";
                return RedirectToAction("Index", "Products", new { area = "Admin" });
            }

            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");

                // Nếu API của bạn nhận PUT với JSON, giữ nguyên dạng này
                var response = await client.PutAsJsonAsync("product-details/bulk-change-status", request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Đã cập nhật trạng thái hàng loạt thành công.";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"{ExtractMessage(error)}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi xử lý: {ex.Message}";
            }

            return RedirectToAction("Index", "Products", new { area = "Admin" });

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile file, Guid? productId)
        {
            // Kiểm tra file
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel để import.";
                return RedirectToAction("Index", "Products", new { area = "Admin" });
            }

            // Kiểm tra ProductId
            if (!productId.HasValue || productId.Value == Guid.Empty)
            {
                TempData["Error"] = "ProductId không hợp lệ.";
                return RedirectToAction("Index", "Products", new { area = "Admin" });
            }

            var client = _httpClientFactory.CreateClient("ApiClient");

            using var form = new MultipartFormDataContent();

            // Thêm file vào form
            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            form.Add(fileContent, "File", file.FileName);

            // Thêm ProductId vào form
            form.Add(new StringContent(productId.Value.ToString()), "ProductId");

            try
            {
                var response = await client.PostAsync("product-details/import-excel", form);

                if (response.IsSuccessStatusCode)
                {
                    var resultContent = await response.Content.ReadAsStringAsync();
                    TempData["Success"] = $"Import Excel thành công: {resultContent}";
                    return RedirectToAction("Index", "Products", new { area = "Admin" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    // Có thể parse JSON message nếu server trả { Message: "...", Detail: "..." }
                    string errorMessage;
                    try
                    {
                        var errorObj = System.Text.Json.JsonDocument.Parse(errorContent);
                        if (errorObj.RootElement.TryGetProperty("Message", out var msg))
                            errorMessage = msg.GetString() ?? "Import thất bại.";
                        else
                            errorMessage = "Import thất bại.";
                    }
                    catch
                    {
                        errorMessage = "Import thất bại.";
                    }

                    TempData["Error"] = errorMessage;
                    return RedirectToAction("Index", "Products", new { area = "Admin" });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi import: {ex.Message}";
                return RedirectToAction("Index", "Products", new { area = "Admin" });
            }
        }

        public async Task<IActionResult> Export()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Gọi API export
            var response = await client.GetAsync("Import/export?entityName=ProductDetail");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Export thất bại: " + await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index");
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"ProductExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        private string ExtractMessage(string rawError)
        {
            try
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(rawError);
                return obj?.message ?? obj?.Message ?? obj?.detail ?? rawError;
            }
            catch
            {
                return rawError;
            }
        }


        private string GetModelStateErrors()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(msg => !string.IsNullOrWhiteSpace(msg));

            return "Lỗi dữ liệu: " + string.Join(" | ", errors);
        }

    }
}
