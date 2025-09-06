using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using API.Domain.Request.AccountRequest;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MVCAuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MVCAuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("ApiClient");

            var loginRequest = new LoginRequest
            {
                UserName = model.UserName,
                Password = model.Password
            };

            var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("Auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonDocument.Parse(resultJson);

                var token = result.RootElement.GetProperty("token").GetString();
                HttpContext.Session.SetString("JWToken", token);

                // Gọi API lấy profile
                var client2 = _httpClientFactory.CreateClient("ApiClient");
                client2.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var profileResponse = await client2.GetAsync("accounts/profile");
                if (profileResponse.IsSuccessStatusCode)
                {
                    var profileJson = await profileResponse.Content.ReadAsStringAsync();
                    HttpContext.Session.SetString("UserData", profileJson);
                }

                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            TempData["ErrorMessage"] = "Tài khoản hoặc mật khẩu không đúng!";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Gọi API logout (chỉ để log / thống kê, nếu bạn đã implement ở AuthController)
                await client.PostAsync("Auth/logout", null);
            }

            HttpContext.Session.Clear();
            await HttpContext.Session.CommitAsync();
            TempData.Clear();
            Response.Cookies.Delete(".AspNetCore.Session");

            return RedirectToAction("Login", "MVCAuth", new { area = "Admin" });
        }


        // ========== FORGOT PASSWORD ==========
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string emailOrUsername)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var content = new StringContent(
                JsonSerializer.Serialize(new { emailOrUsername }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync("Auth/forgot-password", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonDocument.Parse(json);

                // Lấy token và code từ API (chỉ để test)
                var token = result.RootElement.TryGetProperty("token", out var tokenProp) ? tokenProp.GetString() : null;
                var code = result.RootElement.TryGetProperty("code", out var codeProp) ? codeProp.GetString() : null;

                TempData["Token"] = token;
                TempData["Code"] = code;

                TempData["Success"] = "Vui lòng nhập mã OTP và mật khẩu mới.";
                return RedirectToAction("ResetPasswordWithCode", new { token, code });
            }

            TempData["ErrorMessage"] = "Không thể gửi yêu cầu quên mật khẩu.";
            return View();
        }

        // ========== RESET PASSWORD WITH CODE ==========
        [HttpGet]
        public IActionResult ResetPasswordWithCode(string token, string code)
        {
            ViewBag.Token = token;
            ViewBag.Code = code;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordWithCode(string token, string code, string newPassword, string confirmPassword)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var content = new StringContent(
                JsonSerializer.Serialize(new { token, code, newPassword, confirmPassword }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync("Auth/reset-password", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            TempData["ErrorMessage"] = "Mã OTP hoặc token không hợp lệ.";
            return View();
        }
    }
}
