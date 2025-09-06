using API.DomainCusTomer.DTOs.MoMo;
using API.DomainCusTomer.DTOs.ThanhToanCustomer;
using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using MailKit.Search;
using DAL_Empty.Models;
using Org.BouncyCastle.Asn1.X509;
using API.DomainCusTomer.DTOs.QuanLyDonHangCustomerDto;

namespace MVC.Controllers
{
    public class MoMoThanhtoancustomerController : Controller
    {
        private readonly IMomoService _momoService;
        private readonly HttpClient _httpClient;
        public MoMoThanhtoancustomerController(IMomoService momoService, IHttpClientFactory httpClientFactory)
        {
            _momoService = momoService;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7257/api/");
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallBack()
        {
            var username = HttpContext.Request.Cookies["UserName"]
              ?? HttpContext.Request.Cookies["LoginMethod"];

            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Home");
            }
            Console.WriteLine("== MoMo CALLBACK ==\n" + string.Join("\n", Request.Query.Select(q => $"{q.Key} = {q.Value}")));

            if (!_momoService.ValidateSignature(Request.Query))
                return BadRequest("Chữ ký không hợp lệ (Invalid signature)");

            var result = _momoService.PaymentExecuteAsync(Request.Query);
            var jsonData = HttpContext.Session.GetString("MomoPendingOrder");
            var order = JsonSerializer.Deserialize<OrderGuestDto>(jsonData);
            if (result.ErrorCode == "0")
            {

                if (string.IsNullOrEmpty(jsonData))
                    return BadRequest("Không tìm thấy dữ liệu đơn hàng trong session");
                var content = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("ThanhToanCustomer/create-guest-order", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var orderIdObj = JsonSerializer.Deserialize<OrderID>(responseContent);
                    if (order.IsFromCart == true)
                    {
                        Response.Cookies.Delete("CustomerCart");
                        await _httpClient.DeleteAsync("CartCustomer/clear");

                        HttpContext.Session.Remove("MomoPendingOrder");
                        TempData["SuccessMessagethanhtoan"] = "Đặt hàng thành công! mã đơn hàng của bạn là: " + orderIdObj.Id.ToString().ToUpper();
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        HttpContext.Session.Remove("MomoPendingOrder");
                        TempData["SuccessMessagethanhtoan"] = "Đặt hàng thành công! mã đơn hàng của bạn là: " + orderIdObj.Id.ToString().ToUpper();
                        return RedirectToAction("Index", "Home");
                    }

                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var json = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                    string errorMessage = json != null && json.ContainsKey("message") ? json["message"] : "";

                    TempData["Errormomothanhtoan"] = errorMessage + ". Vui lòng liên hệ cửa hàng để được nhận lại tiền.";

                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {

                if (order.IsFromCart == true)
                {
                    return RedirectToAction("IndexThanhToan", "ThanhToanCustomer");
                }
                else
                {
                    return RedirectToAction("IndexMuaNgay", "ThanhToanCustomer");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateGuestOrder(OrderGuestDto request)
        {
            var username = HttpContext.Request.Cookies["UserName"]
              ?? HttpContext.Request.Cookies["LoginMethod"];

            if (!string.IsNullOrEmpty(username))
            {
                // Người dùng đã đăng nhập --> chuyển về trang chính
                return RedirectToAction("Index", "Home");
            }
            if (!ModelState.IsValid)
            {
                if (request.IsFromCart == true)
                {
                    TempData["ErroValidate"] = "Dữ liệu sai định dạng ";
                    return RedirectToAction("IndexThanhToan", "ThanhToanCustomer");
                }
                else
                {
                    TempData["ErroValidate"] = "Dữ liệu sai định dạng ";
                    return RedirectToAction("IndexMuaNgay", "ThanhToanCustomer");
                }
            
            }

            try
            {
                if (request.PaymentMethodCode == "momo")
                {
                    var json = JsonSerializer.Serialize(request);
                    HttpContext.Session.SetString("MomoPendingOrder", json);
                    var paymentResponse = await _momoService.CreatePaymentAsync(new OrderInfoModel
                    {
                        FullName = request.FullName,
                        Amount = (int)Convert.ToDouble(request.TotalAmount),
                        OrderInfo = "Thanh toán sản phẩm tại stylezone "
                    });

                    if (paymentResponse != null && !string.IsNullOrEmpty(paymentResponse.PayUrl))
                    {
                        return Redirect(paymentResponse.PayUrl);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Không nhận được PayUrl từ MoMo.");
                        return RedirectToAction("IndexMuaNgay", "ThanhToanCustomer", request);
                    }
                }
                else if (request.PaymentMethodCode == "cod")
                {
                    // --- Gửi API lưu đơn hàng ---
                    var jsonData = JsonSerializer.Serialize(request);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync("ThanhToanCustomer/create-guest-order", content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var orderIdObj = JsonSerializer.Deserialize<OrderID>(responseContent);

                    if (response.IsSuccessStatusCode)
                    {
                        if (request.IsFromCart == true)
                        {
                            Response.Cookies.Delete("CustomerCart");
                            await _httpClient.DeleteAsync("CartCustomer/clear");

                            TempData["SuccessMessagethanhtoan"] = "Đặt hàng thành công! mã đơn hàng của bạn là: " + orderIdObj.Id.ToString().ToUpper();
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            TempData["SuccessMessagethanhtoan"] = "Đặt hàng thành công! mã đơn hàng của bạn là: " + orderIdObj.Id.ToString().ToUpper();
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        // --- Bắt lỗi từ API: số lượng không đủ ---
                        try
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            var json = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                            string errorMessage = json != null && json.ContainsKey("message") ? json["message"] : "";
                            TempData["Erroaccount"] = errorMessage;
                            return RedirectToAction("Index", "Home");
                        }
                        catch
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            var json = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                            string errorMessage = json != null && json.ContainsKey("message") ? json["message"] : "";
                            TempData["Erroaccount"] = errorMessage;
                            return RedirectToAction("Index", "Home");
                        }


                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"{responseContent}");
                        return RedirectToAction("IndexMuaNgay", "ThanhToanCustomer");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Phương thức thanh toán không hợp lệ.");
                    return RedirectToAction("IndexMuaNgay", "ThanhToanCustomer");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Đã xảy ra lỗi không mong muốn: {ex.Message}");
                return RedirectToAction("IndexMuaNgay", "ThanhToanCustomer");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Tracuudonhang(string orderid)
        {
            if (string.IsNullOrWhiteSpace(orderid))
            {
                ViewBag.Error = "Bạn phải nhập mã đơn hàng";
                return View();
            }

            var response = await _httpClient.GetAsync($"ThanhToanCustomer/tracuudonhang/{orderid}");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Không tìm thấy đơn hàng hoặc lỗi hệ thống";
                return View();
            }

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content) || content == "null")
            {
                // API trả về rỗng hoặc null → không có đơn hàng
                ViewBag.Error = "Không tìm thấy đơn hàng";
                return View();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };

            try
            {
                var result = JsonSerializer.Deserialize<QuanLyDonHangCustomerDto>(content, options);
                if (result == null)
                {
                    ViewBag.Error = "Không tìm thấy đơn hàng";
                    return View();
                }

                return View(result);
            }
            catch (JsonException)
            {
                // Bắt lỗi parse JSON, không cho crash
                ViewBag.Error = "Dữ liệu trả về không hợp lệ";
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> CancelOrder(Guid orderId, string Decription)
        {
            string username = HttpContext.Request.Cookies["UserName"] ?? HttpContext.Request.Cookies["LoginMethod"];
            if (!string.IsNullOrEmpty(username))
            {
                
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var response = await _httpClient.PostAsync(
                    $"DonMuaCustomer/cancelGuest/{orderId}?Decription={Decription}",
                    null);

                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                string message = null;
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Name.Equals("Message", StringComparison.OrdinalIgnoreCase))
                    {
                        message = prop.Value.GetString();
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(message))
                    message = "Không nhận được thông báo từ máy chủ.";

                if (response.IsSuccessStatusCode)
                    TempData["CancelGuestSuccess"] = message;
                else
                    TempData["CancelGuestError"] = message;
            }
            catch (Exception ex)
            {
                TempData["CancelGuestError"] = $"Lỗi khi hủy đơn hàng: {ex.Message}";
            }

            // 👉 Gọi lại API tra cứu đơn hàng để lấy trạng thái mới nhất
            var orderResponse = await _httpClient.GetAsync($"ThanhToanCustomer/tracuudonhang/{orderId}");
            if (!orderResponse.IsSuccessStatusCode)
            {
                ViewBag.Error = "Không tìm thấy đơn hàng hoặc lỗi hệ thống";
                return View("Tracuudonhang");
            }

            var content = await orderResponse.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
            var result = JsonSerializer.Deserialize<QuanLyDonHangCustomerDto>(content, options);

            return View("Tracuudonhang", result); // ✅ Trả lại view với model cập nhật
        }




        [HttpPost]
        public IActionResult MomoNotify([FromForm] IFormCollection collection)
        {
            if (!_momoService.ValidateSignature(collection))
                return BadRequest("Invalid signature");

            var result = _momoService.PaymentExecuteAsync(collection);

            // Cập nhật trạng thái thanh toán trong DB
            if (result.ErrorCode == "0")
            {
                //Thanh toán thành công
                // Ví dụ: _orderService.UpdatePaymentStatus(result.OrderId, true);
            }
            else
            {
                // Thanh toán thất bại
                // _orderService.UpdatePaymentStatus(result.OrderId, false);
            }

            return Ok("success");
        }

    }
}
