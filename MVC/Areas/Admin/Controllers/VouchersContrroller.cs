using API.Domain.DTOs;
using API.Domain.Request.VoucherRequest;
using DAL_Empty.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;

namespace MVC.Controllers
{
    [Area("Admin")]
    public class VouchersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public VouchersController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("voucher");
            if (!response.IsSuccessStatusCode)
                return View(new List<VoucherDto>());

            var content = await response.Content.ReadAsStringAsync();
            var vouchers = JsonConvert.DeserializeObject<List<VoucherDto>>(content) ?? new List<VoucherDto>();
            return View(vouchers);
        }

        public IActionResult Create()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var model = new CreateVoucherRequest
            {
                StartDate = DateTime.Now, // Hoặc DateTime.Today
                EndDate = DateTime.Now.AddDays(7) // Mặc định 7 ngày sau
            };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateVoucherRequest request)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (!ModelState.IsValid)
                return View(request);

            bool hasFile = request.ImageFile != null && request.ImageFile.Length > 0;
            bool hasUrl = !string.IsNullOrWhiteSpace(request.ImageUrl);

            if (!hasFile && !hasUrl)
            {
                TempData["Error"] = "Bạn phải nhập đường dẫn ảnh hoặc tải lên một ảnh.";
                return View(request);
            }

            if (hasFile && hasUrl)
            {
                TempData["Error"] = "Chỉ được chọn một trong hai: Đường dẫn ảnh hoặc tải lên ảnh.";
                return View(request);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                using var content = new MultipartFormDataContent();

                content.Add(new StringContent(request.Code ?? ""), nameof(request.Code));
                content.Add(new StringContent(request.Description ?? ""), nameof(request.Description));
                content.Add(new StringContent(request.DiscountType.ToString()), nameof(request.DiscountType));
                content.Add(new StringContent(request.DiscountValue.ToString()), nameof(request.DiscountValue));

                if (request.MinOrderAmount.HasValue)
                    content.Add(new StringContent(request.MinOrderAmount.Value.ToString()), nameof(request.MinOrderAmount));
                if (request.TotalUsageLimit.HasValue)
                    content.Add(new StringContent(request.TotalUsageLimit.Value.ToString()), nameof(request.TotalUsageLimit));

                content.Add(new StringContent(request.Status.ToString()), nameof(request.Status));
                content.Add(new StringContent(request.StartDate.ToString("o")), nameof(request.StartDate));
                content.Add(new StringContent(request.EndDate.ToString("o")), nameof(request.EndDate));

                if (hasUrl)
                    content.Add(new StringContent(request.ImageUrl), nameof(request.ImageUrl));

                if (hasFile)
                {
                    var stream = request.ImageFile.OpenReadStream();
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ImageFile.ContentType);
                    content.Add(fileContent, nameof(request.ImageFile), request.ImageFile.FileName);
                }

                var response = await client.PostAsync("voucher", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Tạo voucher thành công!";
                    return RedirectToAction(nameof(Index));
                }

                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = ExtractMessage(error);
                return View(request);


            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(request);
            }
        }


        public async Task<IActionResult> Edit(Guid id)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"voucher/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var content = await response.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<VoucherDto>(content);
            if (dto == null) return NotFound();

            var updateModel = new UpdateVoucherRequest
            {
                Id = dto.Id,
                Code = dto.Code,
                Description = dto.Description,
                DiscountType = Enum.Parse<DiscountType>(dto.DiscountType),
                DiscountValue = dto.DiscountValue,
                MinOrderAmount = dto.MinOrderAmount,
                TotalUsageLimit = dto.TotalUsageLimit,
                Status = Enum.Parse<VoucherStatus>(dto.Status),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ImageUrl = dto.ImageUrl
            };

            return View(updateModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateVoucherRequest request)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại!";
                return View(request);
            }

            bool hasFile = request.ImageFile != null && request.ImageFile.Length > 0;
            bool hasUrl = !string.IsNullOrWhiteSpace(request.ImageUrl);


            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                using var content = new MultipartFormDataContent();

                content.Add(new StringContent(request.Id.ToString()), nameof(request.Id));
                content.Add(new StringContent(request.Code ?? ""), nameof(request.Code));
                content.Add(new StringContent(request.Description ?? ""), nameof(request.Description));
                content.Add(new StringContent(request.DiscountType.ToString()), nameof(request.DiscountType));
                content.Add(new StringContent(request.DiscountValue.ToString()), nameof(request.DiscountValue));

                if (request.MinOrderAmount.HasValue)
                    content.Add(new StringContent(request.MinOrderAmount.Value.ToString()), nameof(request.MinOrderAmount));
                if (request.TotalUsageLimit.HasValue)
                    content.Add(new StringContent(request.TotalUsageLimit.Value.ToString()), nameof(request.TotalUsageLimit));

                content.Add(new StringContent(request.Status.ToString()), nameof(request.Status));
                content.Add(new StringContent(request.StartDate.ToString("o")), nameof(request.StartDate));
                content.Add(new StringContent(request.EndDate.ToString("o")), nameof(request.EndDate));

                //if (hasUrl)
                //    content.Add(new StringContent(request.ImageUrl), nameof(request.ImageUrl));


                //if (hasFile)
                //{
                //    var stream = request.ImageFile.OpenReadStream();
                //    var fileContent = new StreamContent(stream);
                //    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ImageFile.ContentType);
                //    content.Add(fileContent, nameof(request.ImageFile), request.ImageFile.FileName);
                //}
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
                var response = await client.PutAsync("voucher", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Cập nhật khuyến mãi thành công!";
                    return RedirectToAction(nameof(Index));
                }

                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = ExtractMessage(error);
                return View(request);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(request);
            }
        }






        [HttpPost]
        public async Task<IActionResult> ChangeStatus(ChangeStatusRequest request)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                using var formdata = new MultipartFormDataContent
                {
                    { new StringContent(request.Status), nameof(request.Status) }
                };
                var apiUrl = $"https://localhost:7257/api/voucher/{request.Id}/change-status";
                var response = await client.PostAsync(apiUrl, formdata);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Cập nhật trạng thái thành công!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = ExtractMessage(error);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkChangeStatus(BulkStatusChangeRequest request)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            if (string.IsNullOrEmpty(request.Status) || request.Ids == null || !request.Ids.Any())
            {
                TempData["Error"] = "Thiếu trạng thái hoặc danh sách ID.";
                return RedirectToAction("Index");
            }

            var client = _httpClientFactory.CreateClient("ApiClient");

            using var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(request.Status), "Status");

            foreach (var id in request.Ids)
            {
                formData.Add(new StringContent(id.ToString()), "Ids");
            }

            try
            {
                var response = await client.PostAsync("voucher/bulk-change-status", formData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Cập nhật trạng thái thành công.";
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Lỗi từ API: {ExtractMessage(errorMsg)}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi gọi API: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Export()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Gọi API export
            var response = await client.GetAsync("Import/export?entityName=Voucher");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Export thất bại: " + await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index");
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"VoucherExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

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
