using API.DomainCusTomer.DTOs.MoMo;
using API.DomainCusTomer.DTOs.ThanhToanCustomer;
using API.DomainCusTomer.DTOs.ThanhToanCustomerId;
using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
namespace MVC.Controllers
{
    public class MoMoThanhtoanCustomerIdController : Controller
    {
        private readonly IMomoCustomerIdServices _momoService;
        private readonly HttpClient _httpClient;
        public MoMoThanhtoanCustomerIdController(IMomoCustomerIdServices momoService, IHttpClientFactory httpClientFactory)
        {
            _momoService = momoService;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7257/api/");
        }
        [HttpGet]
        public async Task<IActionResult> PaymentCallBackId()
        {
            var username = HttpContext.Request.Cookies["UserName"]
              ?? HttpContext.Request.Cookies["LoginMethod"];

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Home");
            }
           
            Console.WriteLine("== MoMo CALLBACK ==\n" + string.Join("\n", Request.Query.Select(q => $"{q.Key} = {q.Value}")));

            if (!_momoService.ValidateSignature(Request.Query))
                return BadRequest("Chữ ký không hợp lệ (Invalid signature)");

            var result = _momoService.PaymentExecuteAsync(Request.Query);
            var jsonData = HttpContext.Session.GetString("MomoOrder");
            var order = JsonSerializer.Deserialize<OrderCustomerIdDto>(jsonData);
            if (result.ErrorCode == "0")
            {

                if (string.IsNullOrEmpty(jsonData))
                    return BadRequest("Không tìm thấy dữ liệu đơn hàng trong session");

                var content = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"ThanhToanCustomerId/create-by-customer-id?username={username}", content);

                if (response.IsSuccessStatusCode)
                {
                    if (order.IsFromCart == true)
                    {
                        var responseremove = await _httpClient.DeleteAsync($"ThanhToanCustomerId/remove-all/{username}");

                        if (responseremove.IsSuccessStatusCode)
                        {
                            var resultremover = await responseremove.Content.ReadFromJsonAsync<dynamic>();
                            return RedirectToAction("ListDonHangPending", "DonMuaCustomer");
                        }
                        else
                        {
                            var error = await response.Content.ReadFromJsonAsync<dynamic>();

                        }
                        TempData["SuccessMessage"] = "Đặt hàng thành công qua MoMo!";
                        return RedirectToAction("ListDonHangPending", "DonMuaCustomer");
                    }
                    else
                    {
                        TempData["Error"] = "Đặt hàng thất bại !";
                        return RedirectToAction("ListDonHangPending", "DonMuaCustomer");
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
                //return RedirectToAction("Index", "Home");
                //return Content($"Thanh toán thất bại. Mã lỗi: {result.ErrorCode} - {result.Message}");

                if (order.IsFromCart == false)
                {
                    return RedirectToAction("IndexMuaNgayID", "ThanhToanCustomerId");
                }
                else
                {
                    return RedirectToAction("ListCartthanhtoanId", "ThanhToanCustomerId");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrderCustomerId(OrderCustomerIdDto request, string username)
        {
            // Lấy username từ cookie
            username = HttpContext.Request.Cookies["UserName"]
                       ?? HttpContext.Request.Cookies["LoginMethod"];

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Home");
            }
            var accountResponse = await _httpClient.GetAsync($"LoginAccountCustomer/check-active/{username}");
            if (!accountResponse.IsSuccessStatusCode)
            {
               
                    Response.Cookies.Delete("UserName");
                    Response.Cookies.Delete("LoginMethod");
                    return RedirectToAction("Index", "Home");
             
            }
            if (request.ShippingFee == 0)
            {
                TempData["MessageDiaChi"] = "Vui lòng chọn địa chỉ";
                return RedirectToAction("ListCartthanhtoanId", "ThanhToanCustomerId");
            }
            try
            {
                // ==== Thanh toán Momo ====
                if (request.PaymentMethodCode == "momo")
                {

                    HttpContext.Session.SetString("MomoOrder", JsonSerializer.Serialize(request));

                    var paymentResponse = await _momoService.CreatePaymentAsync(new OrderInfoModel
                    {
                        FullName = request.FullName,
                        Amount = (int)request.TotalAmount,
                        OrderInfo = "Thanh toán sản phẩm tại stylezone"
                    });

                    if (paymentResponse != null && !string.IsNullOrEmpty(paymentResponse.PayUrl))
                    {
                        return Redirect(paymentResponse.PayUrl);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Không nhận được PayUrl từ MoMo.");
                        return RedirectToAction("IndexMuaNgayID", "ThanhToanCustomerId",request);
                    }


                }

                // ==== Thanh toán COD ====
                if (request.PaymentMethodCode == "cod")
                {
                    var response = await _httpClient.PostAsJsonAsync(
                        $"ThanhToanCustomerId/create-by-customer-id?username={username}",
                        request
                    );

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        var json = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                        string errorMessage = json != null && json.ContainsKey("message") ? json["message"] : "";

                        TempData["ErroaccountId"] = errorMessage;
                        return RedirectToAction("ListCartthanhtoanId", "ThanhToanCustomerId", request);
                    }

                    // Nếu đơn được tạo từ giỏ hàng thì xóa giỏ hàng
                    if (request.IsFromCart == true)
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            var json = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
                            string errorMessage = json != null && json.ContainsKey("message") ? json["message"] : "";

                            TempData["ErroaccountId"] = errorMessage;
                            return RedirectToAction("IndexMuaNgayID", "ThanhToanCustomerId", request);
                        }
                        var removeCartResponse = await _httpClient.DeleteAsync($"ThanhToanCustomerId/remove-all/{username}");
                        if (removeCartResponse.IsSuccessStatusCode)
                        {
                            var result = await removeCartResponse.Content.ReadFromJsonAsync<dynamic>();
                            TempData["Message"] = result?.message ?? "Xóa thành công";
                            TempData["SuccessMessage"] = "Đặt hàng thành công!";
                            return RedirectToAction("ListDonHangPending", "DonMuaCustomer");
                        }
                    }
                    return RedirectToAction("ListDonHangPending", "DonMuaCustomer");
                }
                ModelState.AddModelError(string.Empty, "Phương thức thanh toán không hợp lệ.");
                return RedirectToAction("IndexMuaNgayID", "ThanhToanCustomerId", request);
            }
            catch (Exception ex)
            {
                if (request.IsFromCart == true)
                {
                    return RedirectToAction("ListDonHangPending", "DonMuaCustomer");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Đã xảy ra lỗi không mong muốn: {ex.Message}");
                    return RedirectToAction("IndexMuaNgayID", "ThanhToanCustomerId", request);
                }

            }
        }


        [HttpPost]
        public IActionResult MomoNotifyId([FromForm] IFormCollection collection)
        {
            if (!_momoService.ValidateSignature(collection))
                return BadRequest("Invalid signature");

            var result = _momoService.PaymentExecuteAsync(collection);

            // Cập nhật trạng thái thanh toán trong DB
            if (result.ErrorCode == "0")
            {
                // Thanh toán thành công
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