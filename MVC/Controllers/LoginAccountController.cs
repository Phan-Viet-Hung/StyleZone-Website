using API.DomainCusTomer.DTOs.AccountCustomer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using API.DomainCusTomer.Request.AccountCustomerRequest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Security.Claims;
using API.DomainCusTomer.Request.LoginAccountCustomerRequest;
using DAL_Empty.Models;
using API.DomainCusTomer.Request.Cast;
using API.DomainCusTomer.DTOs.CartICustomer;

namespace MVC.Controllers
{
    public class LoginAccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string CookieCartKey = "CustomerCart";
        public LoginAccountController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7257/api/");
        }

        // ========== SEND OTP FOR PASSWORD RESET ==========
        [HttpGet]
        public IActionResult SendOtp()
        {
            var username = HttpContext.Request.Cookies["UserName"]
                ?? HttpContext.Request.Cookies["LoginMethod"];

            if (!string.IsNullOrEmpty(username))
            {
                // Người dùng đã đăng nhập --> chuyển về trang chính
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.OtpMessage = "Vui lòng nhập email.";
                return View();
            }

            var response = await _httpClient.PostAsync($"LoginAccountCustomer/send-otp?email={email}", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.OtpMessage = "Không thể gửi OTP. Vui lòng thử lại.";
                return View();
            }

            if (responseContent.Contains("Tài khoản chưa tồn tại"))
            {
                ViewBag.OtpMessage = "Email chưa tồn tại trong hệ thống.";
                return View();
            }

            // Lưu Email vào Session
            HttpContext.Session.SetString("EmailForgot", email);
            return RedirectToAction("ConfirmOtpp", "LoginAccount");
        }

        [HttpGet]
        public IActionResult ConfirmOtpp()
        {
            var email = HttpContext.Session.GetString("EmailForgot");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("SendOtp");

            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOtpp(OtpCustomerDto model)
        {
            var email = HttpContext.Session.GetString("EmailForgot");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("SendOtp");

            if (string.IsNullOrWhiteSpace(model.OTP))
            {
                ViewBag.ConfirmOtppnull = "Vui lòng nhập OTP.";
                ViewBag.Email = email;
                return View();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ConfirmOtppnull = "Vui lòng nhập đúng mã OTP.";
                ViewBag.Email = email;
                return View();
            }

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("LoginAccountCustomer/OTP", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ConfirmOtpp = "Mã OTP không đúng";
                ViewBag.Email = email;
                return View();
            }

            // Lưu OTP vào Session
            HttpContext.Session.SetString("OtpForgot", model.OTP);
            return RedirectToAction("ResetPassword", "LoginAccount");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var email = HttpContext.Session.GetString("EmailForgot");
            var otp = HttpContext.Session.GetString("OtpForgot");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
                return RedirectToAction("SendOtp");

            ViewBag.Email = email;
            ViewBag.Otp = otp;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ForgotpasswordCustomerRequest request)
        {
            var email = HttpContext.Session.GetString("EmailForgot");
            var otp = HttpContext.Session.GetString("OtpForgot");

            request.Email = email;
            request.Otp = otp;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(request.NewPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                ViewBag.Email = email;
                ViewBag.Otp = otp;
                return View(request);
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                ViewBag.Error = "Dữ liệu không hợp lệ: " + errors;
                ViewBag.Email = email;
                ViewBag.Otp = otp;
                return View(request);
            }

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("LoginAccountCustomer/reset-password", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiResult = await response.Content.ReadAsStringAsync();
                ViewBag.Error = "Đổi mật khẩu thất bại. Chi tiết: " + apiResult;

                ViewBag.Email = email;
                ViewBag.Otp = otp;
                return View(request);
            }

            // Xóa session sau khi thành công
            HttpContext.Session.Remove("EmailForgot");
            HttpContext.Session.Remove("OtpForgot");
            TempData["Message"] = "Đổi mật khẩu thành công.";

            return RedirectToAction("Login", "LoginAccount");
        }


        // ========== SEND OTP FOR REGISTER ==========
        [HttpGet]
        public IActionResult SendOtppRegister()
        {
            var username = HttpContext.Request.Cookies["UserName"]
                ?? HttpContext.Request.Cookies["LoginMethod"];

            if (!string.IsNullOrEmpty(username))
            {

                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendOtppRegister(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.OtpMessage = "Vui lòng nhập email.";
                return View();
            }

            var response = await _httpClient.PostAsync($"LoginAccountCustomer/send-otpRegister?email={email}", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.OtpMessage = "Không thể gửi OTP. Vui lòng thử lại.";
                return View();
            }

            if (responseContent.Contains("Tài khoản đã tồn tại"))
            {
                ViewBag.OtpMessage = "Email đã tồn tại trong hệ thống.";
                return View();
            }

            // Lưu Email vào Session
            HttpContext.Session.SetString("EmailRegister", email);
            return RedirectToAction("ConfirmOtppRegister", "LoginAccount");
        }

        [HttpGet]
        public IActionResult ConfirmOtppRegister()
        {
            var email = HttpContext.Session.GetString("EmailRegister");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("SendOtppRegister");

            ViewBag.Email1 = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOtppRegister(OtpCustomerDto model)
        {
            var email = HttpContext.Session.GetString("EmailRegister");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("SendOtppRegister");

            if (string.IsNullOrWhiteSpace(model.OTP))
            {
                ViewBag.ConfirmOtppnull = "Vui lòng nhập OTP.";
                ViewBag.Email1 = email;
                return View();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ConfirmOtppnull = "Vui lòng nhập đúng mã OTP";
                ViewBag.Email1 = email;
                return View();
            }

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("LoginAccountCustomer/OTP", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ConfirmOtpp = "Mã OTP không đúng";
                ViewBag.Email1 = email;
                return View();
            }

            // Lưu OTP vào Session
            HttpContext.Session.SetString("OtpVerified", model.OTP);
            return RedirectToAction("Register", "LoginAccount");
        }

        [HttpGet]
        public IActionResult Register()
        {
            var email = HttpContext.Session.GetString("EmailRegister");
            var otp = HttpContext.Session.GetString("OtpVerified");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
                return RedirectToAction("SendOtppRegister");

            ViewBag.Email1 = email;
            ViewBag.Otp1 = otp;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisteCustomerRequest request)
        {
            // Gán Email và Otp từ Session
            request.Email = HttpContext.Session.GetString("EmailRegister") ?? string.Empty;
            request.Otp = HttpContext.Session.GetString("OtpVerified") ?? string.Empty;

            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp))
            {
                TempData["Message"] = "Email hoặc mã OTP không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("SendOtppRegister");
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                ViewBag.Error = "Dữ liệu không hợp lệ: " + errors;
                ViewBag.Email1 = request.Email;
                ViewBag.Otp1 = request.Otp;
                return View(request);
            }

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("LoginAccountCustomer/register", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiResult = await response.Content.ReadAsStringAsync();
                ViewBag.Error = "Đăng ký thất bại hoặc email đã tồn tại. Chi tiết: " + apiResult;

                ViewBag.Email1 = request.Email;
                ViewBag.Otp1 = request.Otp;
                return View(request);
            }

            // Đăng ký thành công -> Clear session
            HttpContext.Session.Remove("EmailRegister");
            HttpContext.Session.Remove("OtpVerified");
            TempData["Message"] = "Đăng ký thành công. Vui lòng đăng nhập.";

            return RedirectToAction("Login", "LoginAccount");
        }


        private async Task MergeGuestCart(string username)
        {
            var guestCartJson = HttpContext.Request.Cookies[CookieCartKey];
            if (string.IsNullOrEmpty(guestCartJson))
                return;

            try
            {
                // 1. Deserialize cookie thành DTO
                var guestCartDto = JsonConvert.DeserializeObject<List<CartCustomerDto>>(guestCartJson) ?? new();

                if (!guestCartDto.Any())
                    return;

                // 2. Map sang request model
                var requests = guestCartDto.Select(x => new CartCustomerRequest
                {
                    ProductDetailcode = x.ProductDetailcode,
                    Quantity = x.Quantity > 0 ? x.Quantity : 1
                }).ToList();

                // 3. Gửi API merge
                var response = await _httpClient.PostAsJsonAsync($"CartCustomerID/merge/{username}", requests);

                if (response.IsSuccessStatusCode)
                {
                    // 4. Xóa cookie nếu merge thành công
                    Response.Cookies.Delete(CookieCartKey);
                    Console.WriteLine("Merge cart thành công và đã xóa cookie.");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Merge cart failed: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error merging cart: {ex.Message}");
            }
        }


        // ========== LOGIN ==========

        [HttpGet]
        public IActionResult Login()
        {
            var username = HttpContext.Request.Cookies["UserName"]
                ?? HttpContext.Request.Cookies["LoginMethod"];

            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginnCustomerRequest request)
        {
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("LoginAccountCustomer/login", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                ViewBag.Error = errorMsg;
                return View(request);
            }
            HttpContext.Response.Cookies.Append("UserName", request.UserName ?? string.Empty, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });

            await MergeGuestCart(request.UserName);
            TempData["Message"] = "Đăng nhập thành công";
            return RedirectToAction("Index", "Home");
        }

        public async Task LoginByGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var username = HttpContext.Request.Cookies["UserName"]
               ?? HttpContext.Request.Cookies["LoginMethod"];

            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal == null)
            {
                TempData["error"] = "Xác thực Google thất bại.";
                return RedirectToAction("Login");
            }

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(email))
            {
                TempData["error"] = "Google không trả về email.";
                return RedirectToAction("Login");
            }

            var request = new LoginGoogleCustomerRequest
            {
                Email = email,
                Name = name,
                UserName = email.Split('@')[0]
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("LoginAccountCustomer/LoginGoole", request);

                if (response.IsSuccessStatusCode)
                {
                    // ✅ chỉ set cookie khi login Google thành công
                    HttpContext.Response.Cookies.Append("LoginMethod", request.UserName, new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(7),
                        HttpOnly = false,
                        Secure = true,
                        SameSite = SameSiteMode.Lax
                    });

                    await MergeGuestCart(request.UserName);
                    var customer = await response.Content.ReadFromJsonAsync<Customer>();
                    if (customer != null)
                    {
                        TempData["successs"] = "Đăng nhập Google thành công!";
                    }
                    else
                    {
                        TempData["errorgoogle"] = "Không đọc được dữ liệu trả về từ API.";
                        return RedirectToAction("Login");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    TempData["errorgoogle"] = $"Đăng nhập thất bại: {errorMsg}";
                    return RedirectToAction("Login");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    TempData["errorgoogle"] = $"Yêu cầu không hợp lệ: {errorMsg}";
                    return RedirectToAction("Login");
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    TempData["errorgoogle"] = $"Lỗi hệ thống: {errorMsg}";
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["errorgoogle"] = $"Lỗi kết nối tới API: {ex.Message}";
                return RedirectToAction("Login");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // Xóa các cookies mà bạn đã lưu khi login
            if (Request.Cookies.ContainsKey("UserName"))
                Response.Cookies.Delete("UserName");

            if (Request.Cookies.ContainsKey("LoginMethod"))
                Response.Cookies.Delete("LoginMethod");

            return RedirectToAction("Index", "Home");
        }

    }
}
