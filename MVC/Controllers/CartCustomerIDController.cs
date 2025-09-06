using API.DomainCusTomer.Request.Cast;
using DAL_Empty.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json;
using API.DomainCusTomer.DTOs.CastCustomerId;
using API.DomainCusTomer.DTOs.ThongTinCaNhaCustomer;

namespace MVC.Controllers
{
    public class CartCustomerIDController : Controller
    {
        private readonly HttpClient _httpClient;

        public CartCustomerIDController()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7257/api/")
            };
        }
        [HttpGet]
        public async Task<IActionResult> ListCartId(string username)
        {
            username = HttpContext.Request.Cookies["UserName"] ?? HttpContext.Request.Cookies["LoginMethod"];

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "Home");

            try
            {
                var response = await _httpClient.GetAsync($"CartCustomerID/{username}");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Không thể lấy giỏ hàng.";
                    return View(new List<CastCustomerIDDto>());
                }

                var cartItems = await response.Content.ReadFromJsonAsync<List<CastCustomerIDDto>>();
                return View(cartItems ?? new List<CastCustomerIDDto>());
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return View(new List<CastCustomerIDDto>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCartID(string username, CartCustomerRequest request)
        {
            username = HttpContext.Request.Cookies["UserName"] ?? HttpContext.Request.Cookies["LoginMethod"];

            if (string.IsNullOrEmpty(username))
                return BadRequest("Không tìm thấy thông tin người dùng.");

            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"CartCustomerID/{username}", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("ListCartId");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorObj = JsonConvert.DeserializeObject<dynamic>(errorContent);
                        TempData["Error"] = errorObj?.error ?? "Lỗi không xác định.";
                    }
                    catch
                    {
                        TempData["Error"] = errorContent;
                    }

                    return RedirectToAction("ListCartId");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi hệ thống: {ex.Message}";
                return RedirectToAction("ListCartId");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Increase(Guid Id)
        {
            var response = await _httpClient.PostAsync($"CartCustomerID/increase/{Id}", null);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("ListCartId");

            TempData["ErrorMessage"] = "Không thể tăng số lượng.";
            return RedirectToAction("ListCartId");
        }

        [HttpPost]
        public async Task<IActionResult> Decrease(Guid Id)
        {
            var response = await _httpClient.PostAsync($"CartCustomerID/decrease/{Id}", null);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("ListCartId");

            TempData["ErrorMessage"] = "Không thể giảm số lượng.";
            return RedirectToAction("ListCartId");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var response = await _httpClient.DeleteAsync($"CartCustomerID/{Id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction("ListCartId");

            TempData["ErrorMessage"] = "Không thể xóa sản phẩm.";
            return RedirectToAction("ListCartId");
        }
        [HttpPost]
        public async Task<IActionResult> BuyNowID(string username, CartCustomerRequest request)
        {
           
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"api/CartCustomer/buynow/{username}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đã mua ngay thành công!";
                return RedirectToAction("ListCartId", "CartCustomerID"); 
            }

            var error = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = "Không thể mua ngay: " + error;
            return RedirectToAction("DetailCustomer", "DetailCustomer"); 
        }


        [HttpGet]
        public async Task<IActionResult> CartBeforeCheckout()
        {
            var username = HttpContext.Request.Cookies["UserName"] ?? HttpContext.Request.Cookies["LoginMethod"];

            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện thao tác này.";
                return RedirectToAction("ListCartId");
            }

            try
            {
                var validateResponse = await _httpClient.GetAsync($"CartCustomerID/validate-quantity?username={username}");

                var json = await validateResponse.Content.ReadAsStringAsync();

                if (validateResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // Phân tích lỗi từ API
                    var result = JsonConvert.DeserializeObject<dynamic>(json);
                    var errors = ((Newtonsoft.Json.Linq.JArray)result.errors).ToObject<List<string>>();

                    // Lưu lỗi vào TempData để hiển thị trên trang
                    TempData["CartValidationErrors"] = string.Join(", ", errors);
                    return RedirectToAction("ListCartId");
                }

                if (!validateResponse.IsSuccessStatusCode)
                {
                    // Xử lý lỗi không thành công khác
                    var errorMessage = "Lỗi khi kiểm tra giỏ hàng.";
                    if (!string.IsNullOrEmpty(json))
                    {
                        var errorDetails = JsonConvert.DeserializeObject<dynamic>(json);
                        errorMessage += $" Chi tiết: {errorDetails.message}"; // Thay đổi tùy theo cấu trúc JSON
                    }
                    TempData["ErrorMessage"] = errorMessage;
                    return RedirectToAction("ListCartId");
                }

                // ✅ Nếu hợp lệ, qua trang thanh toán
                return RedirectToAction("ListCartthanhtoanId", "ThanhToanCustomerId");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi: " + ex.Message;
                return RedirectToAction("ListCartId");
            }
        }
        [HttpPost]
        public async Task<IActionResult> ThemDiaChiMuaNgay(DiachiCustomerDto newAddress)
        {
            var username = HttpContext.Request.Cookies["UserName"] ?? HttpContext.Request.Cookies["LoginMethod"];

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
                return RedirectToAction("IndexMuaNgayID", "ThanhToanCustomerId");
            }

            var response = await _httpClient.PostAsync(
                $"DonMuaCustomer/{username}/address",
                new StringContent(JsonConvert.SerializeObject(newAddress), Encoding.UTF8, "application/json")
            );

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Thêm địa chỉ thành công!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Thêm địa chỉ thất bại: {error}";
            }

            return RedirectToAction("IndexMuaNgayID", "ThanhToanCustomerId");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateDiaChiMuaNgay(DiachiCustomerDto model, Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["Error"] = "ID địa chỉ không hợp lệ.";
                return RedirectToAction("IndexMuaNgayID", "ThanhToanCustomerId");
            }

            var response = await _httpClient.PutAsJsonAsync($"DonMuaCustomer/address/{id}", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Cập nhật địa chỉ thành công!";
               
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Lỗi khi cập nhật địa chỉ: {errorMessage}";
              
            }
            return RedirectToAction("IndexMuaNgayID", "ThanhToanCustomerId");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatusDiaChiMuaNgay(Guid id, string username)
        {
            // Lấy username từ cookie
            username = HttpContext.Request.Cookies["UserName"] ?? HttpContext.Request.Cookies["LoginMethod"];

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "Home");

            try
            {
                // Gửi request PUT sang API
                var apiUrl = $"DonMuaCustomer/UpdateStatusDiaChi/{username}/{id}";
                var response = await _httpClient.PostAsync(apiUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Cập nhật địa chỉ mặc định thành công.";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Lỗi: {error}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi hệ thống: {ex.Message}";
            }

            return RedirectToAction("IndexMuaNgayID", "ThanhToanCustomerId");
        }

    }
}
