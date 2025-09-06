using System.Net;
using System.Net.Http.Headers;
using System.Text;
using API.Domain.DTOs;
using API.Domain.Request.AccountRequest;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        // 🔹 Helper kiểm tra đăng nhập
        private IActionResult RedirectIfNotAuthenticated()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "MVCAuth", new { area = "Admin" });
            }
            return null;
        }

        [HttpGet]
        public IActionResult TestAuth()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            var redirect = RedirectIfNotAuthenticated();
            if (redirect != null) return redirect;

            if (User.Identity?.IsAuthenticated == true)
            {
                return Content($"✅ Đã login: {User.Identity.Name}");
            }
            else
            {
                return Content("❌ Chưa login");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            //var redirect = RedirectIfNotAuthenticated();
            //if (redirect != null) return redirect;

            var response = await _httpClient.GetAsync("accounts");
            if (!response.IsSuccessStatusCode)
                return View(new List<AccountDto>());

            var json = await response.Content.ReadAsStringAsync();
            var accounts = JsonConvert.DeserializeObject<List<AccountDto>>(json);
            return View(accounts);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string phoneNumber)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            //var redirect = RedirectIfNotAuthenticated();
            //if (redirect != null) return redirect;

            var response = await _httpClient.GetAsync($"accounts/by-phone/{phoneNumber}");
            if (!response.IsSuccessStatusCode)
            {
                await LoadRolesToViewBag();
                HandleErrorResponse(response);
                return View("Error");
            }

            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<AccountDto>(json);
            await LoadRolesToViewBag();
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            //var redirect = RedirectIfNotAuthenticated();
            //if (redirect != null) return redirect;

            await LoadRolesToViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAccountRequest request)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            //var redirect = RedirectIfNotAuthenticated();
            //if (redirect != null) return redirect;

            if (!ModelState.IsValid)
            {
                await LoadRolesToViewBag();
                return View(request);
            }

            try
            {
                var jsonRequest = JsonConvert.SerializeObject(new List<CreateAccountRequest> { request });
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("accounts", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Tạo tài khoản thành công!";
                    return RedirectToAction(nameof(Index));
                }

                var errorResponse = await response.Content.ReadAsStringAsync();
                string errorMessage;

                try
                {
                    dynamic errorObj = JsonConvert.DeserializeObject(errorResponse);
                    errorMessage = errorObj?.Message ?? errorResponse;
                }
                catch
                {
                    errorMessage = errorResponse;
                }

                TempData["Error"] = "Tạo tài khoản thất bại: ";
                ModelState.AddModelError(string.Empty, errorMessage);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi hệ thống: " + ex.Message;
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            await LoadRolesToViewBag();
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActiveStatus(List<SetActiveStatusRequest> accounts)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            //var redirect = RedirectIfNotAuthenticated();
            //if (redirect != null) return redirect;

            try
            {
                var json = JsonConvert.SerializeObject(accounts);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync("accounts/bulk-set-active", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Cập nhật trạng thái thành công.";
                    return RedirectToAction("Index");
                }

                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = "Cập nhật thất bại: " + error;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Route("Admin/Account/Toggle/{id}")]
        public async Task<IActionResult> Toggle(Guid id)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            //var redirect = RedirectIfNotAuthenticated();
            //if (redirect != null) return redirect;

            try
            {

                var response = await _httpClient.PutAsync($"accounts/{id}/toggle-active", null);

                if (response.IsSuccessStatusCode)
                {

                    return Ok(new { Message = "Cập nhật trạng thái thành công" });
                }



                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { Message = "Lỗi khi cập nhật", Error = error });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống", Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            //var redirect = RedirectIfNotAuthenticated();
            //if (redirect != null) return redirect;

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn tệp Excel hợp lệ.";
                return RedirectToAction("Index");
            }

            using var stream = file.OpenReadStream();
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("accounts/import-excel", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                dynamic result = JsonConvert.DeserializeObject(responseBody);
                int success = result.successCount;
                int skipped = result.skippedCount;
                var errors = result.errors;

                if (success == 0)
                {
                    TempData["Error"] = "Không thêm được tài khoản nào. Tất cả đều bị trùng hoặc có lỗi dữ liệu.";
                }
                else
                {
                    TempData["Success"] = $"✅ Đã thêm {success} tài khoản.";
                    if (skipped > 0)
                        TempData["Warning"] = $"⛔ {skipped} dòng bị bỏ qua do trùng email hoặc số điện thoại.";
                    if (errors != null && errors.Count > 0)
                        TempData["Warning"] += "<br/>⚠️ Một số dòng có lỗi:<br/>- " + string.Join("<br/>- ", errors);
                }
            }
            else
            {
                TempData["Error"] = "Import thất bại: " + responseBody;
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProfile(Guid id)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            //var redirect = RedirectIfNotAuthenticated();
            //if (redirect != null) return redirect;

            var response = await _httpClient.GetAsync($"accounts/{id}");
            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response);
                return View("Error");
            }

            var json = await response.Content.ReadAsStringAsync();
            var account = JsonConvert.DeserializeObject<AccountDto>(json);

            await LoadRolesToViewBag();
            return View(account); // View sẽ dùng AccountDto làm model
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            //var redirect = RedirectIfNotAuthenticated();
            //if (redirect != null) return redirect;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ!";
                return RedirectToAction("Profile");
            }

            try
            {
                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync("accounts/profile", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Cập nhật tài khoản thành công!";
                    return RedirectToAction(nameof(Profile));
                }

                var errorResponse = await response.Content.ReadAsStringAsync();
                string errorMessage;
                try
                {
                    dynamic errorObj = JsonConvert.DeserializeObject(errorResponse);
                    errorMessage = errorObj?.message ?? errorResponse;
                }
                catch
                {
                    errorMessage = errorResponse;
                }

                TempData["Error"] = "Cập nhật thất bại: " + errorMessage;
                ModelState.AddModelError(string.Empty, errorMessage);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi hệ thống: " + ex.Message;
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var a = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(a))
                return RedirectToAction("Login", "MVCAuth");
            //var redirect = RedirectIfNotAuthenticated();
            //if (redirect != null) return redirect;

            var response = await _httpClient.GetAsync("accounts/profile");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không thể tải thông tin cá nhân";
                return View("Error");
            }

            var data = await response.Content.ReadAsStringAsync();
            var account = JsonConvert.DeserializeObject<AccountDto>(data);
            return View(account);
        }

        #region Helper Methods

        private async Task LoadRolesToViewBag()
        {
            var response = await _httpClient.GetAsync("accounts/roles");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Roles = new List<RoleDto>();
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            var roles = JsonConvert.DeserializeObject<List<RoleDto>>(json);
            ViewBag.Roles = roles;
        }

        private void HandleErrorResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
                ModelState.AddModelError(string.Empty, "Không tìm thấy tài khoản.");
            else
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi truy vấn tài khoản.");
        }

        #endregion
    }
}
