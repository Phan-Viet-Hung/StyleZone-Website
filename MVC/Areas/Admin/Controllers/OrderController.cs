using API.Domain.DTOs;
using API.Domain.Request.OrderRequest;
using DAL_Empty.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace MVC.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class OrderController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "orderapi";

        public OrderController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        // GET: /Admin/Order
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var orders = await GetApiResponse<List<OrderDto>>($"{_apiBaseUrl}");
            if (orders == null)
            {
                TempData["Error"] = "Không thể tải danh sách đơn hàng.";
                return View(new List<OrderDto>());
            }
            return View(orders);
        }

        // GET: /Admin/Order/Details/{id}?print=true
        [HttpGet]
        public async Task<IActionResult> Details(Guid id, bool print = false)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            // Lấy đơn hàng (đã bao gồm BillHistories trong JSON)
            var order = await GetApiResponse<OrderDto>($"{_apiBaseUrl}/{id}");
            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại.";
                return View("Error");
            }

            // Lấy lý do hủy từ BillHistories
            string? cancelReason = null;
            if (order.BillHistories != null && order.BillHistories.Any())
            {
                var lastHistory = order.BillHistories
                    .OrderByDescending(h => h.CreateAt)
                    .FirstOrDefault(h => !string.IsNullOrEmpty(h.Description));

                if (lastHistory != null)
                    cancelReason = lastHistory.Description;
            }

            ViewBag.CancelReason = cancelReason;

            if (print)
                return View("Print", order);

            return View(order);
        }
        [HttpGet]
        public async Task<IActionResult> OrderDetailNew(Guid id, bool print = false)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            // Lấy đơn hàng (đã bao gồm BillHistories trong JSON)
            var order = await GetApiResponse<OrderDto>($"{_apiBaseUrl}/{id}");
            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại.";
                return View("Error");
            }

            // Lấy lý do hủy từ BillHistories
            string? cancelReason = null;
            if (order.BillHistories != null && order.BillHistories.Any())
            {
                var lastHistory = order.BillHistories
                    .OrderByDescending(h => h.CreateAt)
                    .FirstOrDefault(h => !string.IsNullOrEmpty(h.Description));

                if (lastHistory != null)
                    cancelReason = lastHistory.Description;
            }

            ViewBag.CancelReason = cancelReason;

            if (print)
                return View("Print", order);

            return View(order);
        }






        [HttpGet]
        public async Task<IActionResult> Print(Guid id)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var order = await GetApiResponse<OrderDto>($"{_apiBaseUrl}/{id}");
            if (order == null)
                return NotFound();

            return View(order); // View: Print.cshtml
        }

        // GET: /Admin/Order/Create
        [HttpGet]
        public async Task<IActionResult> Create(int tabId = 1)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            await PrepareViewBags();

            // Lấy draft từ Session
            CreateOrderRequest draft = null;
            var json = HttpContext.Session.GetString($"Draft_{tabId}");
            if (!string.IsNullOrEmpty(json))
            {
                draft = JsonConvert.DeserializeObject<CreateOrderRequest>(json);
            }

            if (draft == null)
            {
                draft = new CreateOrderRequest();
                HttpContext.Session.SetString($"Draft_{tabId}", JsonConvert.SerializeObject(draft));
            }

            ViewBag.TabId = tabId;
            ViewBag.OpenTabs = GetOpenTabs();
            return View(draft);
        }

        // POST: lưu draft (gọi ajax hoặc submit)
        [HttpPost]
        public IActionResult SaveDraft(int tabId, CreateOrderRequest draft)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var json = JsonConvert.SerializeObject(draft);
            HttpContext.Session.SetString($"Draft_{tabId}", json);
            return Json(new { success = true });
        }

        // POST: Thanh toán (commit hóa đơn thật)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(int tabId)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            // 1️⃣ Lấy draft từ Session
            var json = HttpContext.Session.GetString($"Draft_{tabId}");
            if (string.IsNullOrEmpty(json))
            {
                TempData["Error"] = "Không tìm thấy hóa đơn nháp.";
                return RedirectToAction(nameof(Index));
            }

            var draft = JsonConvert.DeserializeObject<CreateOrderRequest>(json);

            // 2️⃣ Gọi API tạo hóa đơn
            var postResult = await PostApiResponseWithData<Guid>($"{_apiBaseUrl}", draft);

            if (!postResult.IsSuccess || postResult.Data == Guid.Empty)
            {
                return Json(new { success = false, message = $"Tạo hóa đơn thất bại: {postResult.ErrorMessage}" });
            }

            var createdOrderId = postResult.Data;

            // 3️⃣ Xóa tab vừa tạo
            HttpContext.Session.Remove($"Draft_{tabId}");

            // 4️⃣ Cập nhật lại số tab còn lại liên tiếp
            var openTabs = GetOpenTabs(); // danh sách tab cũ
            openTabs.Sort();

            var remainingDrafts = new List<CreateOrderRequest>();
            foreach (var t in openTabs)
            {
                if (t != tabId)
                {
                    var draftJson = HttpContext.Session.GetString($"Draft_{t}");
                    if (!string.IsNullOrEmpty(draftJson))
                    {
                        var d = JsonConvert.DeserializeObject<CreateOrderRequest>(draftJson);
                        remainingDrafts.Add(d);
                    }
                    HttpContext.Session.Remove($"Draft_{t}");
                }
            }

            // Set lại tab theo thứ tự liên tiếp
            for (int i = 0; i < remainingDrafts.Count; i++)
            {
                HttpContext.Session.SetString($"Draft_{i + 1}", JsonConvert.SerializeObject(remainingDrafts[i]));
            }

            // 5️⃣ Lấy lại order vừa tạo để render partial view
            var order = await GetApiResponse<OrderDto>($"{_apiBaseUrl}/{createdOrderId}");
            if (order == null)
            {
                return Json(new { success = false, message = "Không load được đơn hàng sau khi tạo." });
            }

            // 6️⃣ Trả về partial view hiển thị modal
            return PartialView("OrderDetailNew", order);
        }



        [HttpGet]
        public async Task<IActionResult> OrderDetailNew(Guid id)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var order = await GetApiResponse<OrderDto>($"{_apiBaseUrl}/{id}");
            if (order == null)
                return Content("Không tìm thấy đơn hàng");

            return PartialView("OrderDetailNew", order);
        }

        // GET: mở tab mới
        [HttpGet]
        public IActionResult NewTab()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            int newTabId = 1;
            for (int i = 1; i <= 10; i++)
            {
                if (HttpContext.Session.GetString($"Draft_{i}") == null)
                {
                    newTabId = i;
                    HttpContext.Session.SetString($"Draft_{i}", JsonConvert.SerializeObject(new CreateOrderRequest()));
                    break;
                }
            }
            return RedirectToAction("Create", new { tabId = newTabId });
        }

        // Helper: lấy danh sách tab đang mở
        private List<int> GetOpenTabs()
        {
            var tabs = new List<int>();
            for (int i = 1; i <= 10; i++)
            {
                if (HttpContext.Session.GetString($"Draft_{i}") != null)
                    tabs.Add(i);
            }
            return tabs;
        }
        // POST: cập nhật trạng thái đơn lẻ
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid orderId, int status, string? reason)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "MVCAuth");

            AddAuthHeader();

            var request = new
            {
                OrderId = orderId,
                Status = status,
                Reason = reason
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/update-status", content);
            var resultMsg = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Cập nhật trạng thái thành công!";
            }
            else
            {
                try
                {
                    // Parse lỗi JSON từ API
                    var errorObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(resultMsg);
                    if (errorObj != null && errorObj.ContainsKey("message"))
                    {
                        TempData["Error"] = errorObj["message"];
                    }
                    else
                    {
                        TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái.";
                    }
                }
                catch
                {
                    // Nếu không parse được JSON thì fallback ra raw text
                    TempData["Error"] = $"Có lỗi xảy ra: {resultMsg}";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
public async Task<IActionResult> UpdateStatusBulk(List<Guid> orderIds, int status)
{
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            AddAuthHeader();

    var request = new
    {
        OrderIds = orderIds,
        Status = status
    };
            if(request.Status == 0)
            {
                TempData["Error"] = "Vui lòng chọn trạng thái";
            }
    var jsonRequest = JsonConvert.SerializeObject(request);
    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

    var response = await _httpClient.PutAsync($"{_apiBaseUrl}/update-status-bulk", content);
    var resultMsg = await response.Content.ReadAsStringAsync();

    if (response.IsSuccessStatusCode)
    {
        TempData["Success"] = "Cập nhật trạng thái hàng loạt thành công!";
    }
    else
    {
        // Thử parse JSON từ resultMsg
        try
        {
            var errorObj = JsonConvert.DeserializeObject<dynamic>(resultMsg);
            string message = errorObj?.message ?? resultMsg; // nếu không có message thì giữ nguyên
            TempData["Error"] = $"Lỗi cập nhật hàng loạt: {message}";
        }
        catch
        {
            TempData["Error"] = $"Lỗi cập nhật hàng loạt: {resultMsg}";
        }
    }

    return RedirectToAction(nameof(Index));
}

        [HttpGet]
        public IActionResult CancelOrder(Guid id)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var model = new UpdateOrderStatusRequest
            {
                OrderId = id
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(UpdateOrderStatusRequest model)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            AddAuthHeader();

            var request = new
            {
                OrderId = model.OrderId,
                Status = 6, // Cancelled
                Reason = model.Reason
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/update-status", content);
            var resultMsg = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Đã hủy đơn hàng thành công!";
            else
                TempData["Error"] = $"Lỗi hủy: {response.StatusCode} - {resultMsg}";

            return RedirectToAction(nameof(Index));
        }

        #region Helpers

        private void AddAuthHeader()
        {
            var token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private async Task PrepareViewBags()
        {
            var products = await GetApiResponse<List<ProductDetailDto>>("product-details/with-price");
            var modeofpayments = await GetApiResponse<List<ModeOfPayment>>("modeofpayment");

            ViewBag.Products = products ?? new List<ProductDetailDto>();

            ViewBag.ModeOfPayments = modeofpayments?.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();
        }

        private async Task<T> GetApiResponse<T>(string endpoint)
        {
            try
            {
                AddAuthHeader();

                var response = await _httpClient.GetAsync(endpoint);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return default;

                if (!response.IsSuccessStatusCode)
                    return default;

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi hệ thống: {ex.Message}";
                return default;
            }
        }

        private async Task<(bool IsSuccess, string ErrorMessage)> PostApiResponse<T>(string endpoint, T request)
        {
            try
            {
                AddAuthHeader();

                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorResponse = await response.Content.ReadAsStringAsync();
                dynamic errorObj = JsonConvert.DeserializeObject(errorResponse);
                string errorMessage = errorObj?.message ?? errorResponse;

                return (false, errorMessage);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        private async Task<(bool IsSuccess, string ErrorMessage, T Data)> PostApiResponseWithData<T>(string endpoint, object request)
        {
            try
            {
                AddAuthHeader();

                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        dynamic errorObj = JsonConvert.DeserializeObject(responseBody);
                        string errorMessage = errorObj?.message ?? responseBody;
                        return (false, errorMessage, default);
                    }
                    catch
                    {
                        return (false, responseBody, default);
                    }
                }

                if (string.IsNullOrWhiteSpace(responseBody))
                    return (true, null, default);

                try
                {
                    var jo = JsonConvert.DeserializeObject<JObject>(responseBody);
                    if (jo != null)
                    {
                        if (jo["id"] != null)
                        {
                            var idToken = jo["id"];

                            if (typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?))
                            {
                                if (idToken.Type == JTokenType.String && Guid.TryParse(idToken.ToString(), out var g))
                                    return (true, null, (T)(object)g);

                                if (idToken.Type == JTokenType.Guid)
                                    return (true, null, (T)(object)idToken.ToObject<Guid>());
                            }

                            if (idToken.Type == JTokenType.Object && idToken["id"] != null)
                            {
                                var nestedIdStr = idToken["id"].ToString();
                                if (typeof(T) == typeof(Guid) && Guid.TryParse(nestedIdStr, out var nestedGuid))
                                    return (true, null, (T)(object)nestedGuid);
                            }

                            try
                            {
                                return (true, null, idToken.ToObject<T>());
                            }
                            catch
                            {
                                try
                                {
                                    var val = Convert.ChangeType(idToken.ToString(), typeof(T));
                                    return (true, null, (T)val);
                                }
                                catch
                                {
                                    return (true, null, default);
                                }
                            }
                        }

                        try
                        {
                            return (true, null, jo.ToObject<T>());
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }

                try
                {
                    return (true, null, JsonConvert.DeserializeObject<T>(responseBody));
                }
                catch
                {
                    return (true, null, default);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}", default);
            }
        }

        #endregion
        public async Task<IActionResult> Export()
        {
          

            // Gọi API export
            var response = await _httpClient.GetAsync("Import/export?entityName=OrderInfo");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Export thất bại: " + await response.Content.ReadAsStringAsync();
                return RedirectToAction("Index");
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"OrderInfoExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}
