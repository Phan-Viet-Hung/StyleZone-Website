using API.DomainCusTomer.DTOs.AccountCustomer;
using API.DomainCusTomer.Request.AccountCustomerRequest;
using API.DomainCusTomer.Request.LoginAccountCustomerRequest;
using API.DomainCusTomer.Services;
using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.ComponentModel.DataAnnotations;
using static System.Net.WebRequestMethods;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginAccountCustomerController : ControllerBase
    {
        private readonly ILoginAccountCustomerServices _accountService;
        private readonly EmailCustomerServicer _emailService;
        private readonly OtpHelperServices _otpHelper;
        private readonly ILogger<LoginAccountCustomerController> _logger;

        public LoginAccountCustomerController(
            ILoginAccountCustomerServices accountService,
            EmailCustomerServicer emailService,
            OtpHelperServices otpHelper,
            ILogger<LoginAccountCustomerController> logger)
        {
            _accountService = accountService;
            _emailService = emailService;
            _otpHelper = otpHelper;
            _logger = logger;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromQuery][EmailAddress] string email)
        {
            var a = _accountService.CheckEmail(email);
            if (a == false)
            {
                var otp = _otpHelper.Generate(email);
                try
                {
                    await _emailService.SendOtpAsync(email, otp);
                    return Ok("Mã OTP đã được gửi qua email.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi gửi email OTP cho {Email}", email);
                    return StatusCode(500, "Không gửi được email. Vui lòng thử lại sau.");
                }
            }
            else
            {
                return Ok("Tài khoản chưa tồn tại");
            }

        }
        [HttpPost("send-otpRegister")]
        public async Task<IActionResult> SendOtpRegister([FromQuery][EmailAddress] string email)
        {
            var a = _accountService.CheckEmail(email);
            if (a == true)
            {
                var otp = _otpHelper.Generate(email);
                try
                {
                    await _emailService.SendOtpAsync(email, otp);
                    return Ok("Mã OTP đã được gửi qua email.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi gửi email OTP cho {Email}", email);
                    return StatusCode(500, "Không gửi được email. Vui lòng thử lại sau.");
                }
            }
            else
            {
                return Ok("Tài khoản đã tồn tại ");
            }

        }
        [HttpPost("OTP")]
        public async Task<IActionResult> OTP([FromBody] OtpCustomerDto request)
        {
            if (!_otpHelper.Verify(request.Email, request.OTP))
                return BadRequest("Mã OTP không hợp lệ hoặc đã hết hạn.");
            return Ok("Xác minh OTP thành công.");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisteCustomerRequest request)
        {


            bool isValidOtp = _otpHelper.Verify(request.Email, request.Otp);
            if (!isValidOtp)
            {
                return BadRequest("Thêm tài khoản không thành công");
            }
            var result = await _accountService.RegisterAsync(request);
            if (result == null)
            {
                return BadRequest("Thêm tài khoản không thành công");
            }
            return Ok("Thêm tài khoản thành công");
        }



        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ForgotpasswordCustomerRequest request)
        {

            bool isValidOtp = _otpHelper.Verify(request.Email, request.Otp);
            if (!isValidOtp)
            {
                return BadRequest("Mã OTP không hợp lệ hoặc đã hết hạn.");
            }
            var result = await _accountService.forgotpassword(request);
            if (result == null)
            {
                return BadRequest("Không thể cập nhật mật khẩu.");
            }

            return Ok("Đổi mật khẩu thành công.");
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginnCustomerRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var token = await _accountService.LoginAsync(request);
                if (token == null)
                    return Unauthorized("Tên đăng nhập hoặc mật khẩu không đúng.");

                return Ok("Đăng nhập thành công");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("LoginGoole")]
        public async Task<IActionResult> LoginGoole([FromBody] LoginGoogleCustomerRequest request)
        {
            try
            {
                var success = await _accountService.LoginGoole(request);
                return Ok(success);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex) // ví dụ lỗi do check tài khoản
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex) // lỗi không đoán trước
            {
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }
        [HttpGet("check-active/{username}")]
        public async Task<IActionResult> CheckActive(string username)
        {
            try
            {
            
                await _accountService.GetByUsernameAsync(username);

                // Nếu chạy tới đây tức là tài khoản tồn tại và đang active
                return Ok(new
                {
                    Success = true,
                    Message = "Tài khoản đang hoạt động"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Lỗi hệ thống",
                    Error = ex.Message
                });
            }
        }
    }
}
