using API.Domain.Request.AccountRequest;
using API.Domain.Service.IService;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = await _authService.LoginAsync(request.UserName, request.Password);
            return Ok(new { token });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.EmailOrUsername))
            return BadRequest(new { message = "EmailOrUsername is required." });

        try
        {
            var resp = await _authService.ForgotPasswordAsync(request.EmailOrUsername);
            // Production: return Ok() only
            return Ok(new
            {
                token = resp.ResetToken, // key "token" để MVC đọc
                code = resp.Code,
                message = resp.Message
            }); // resp contains Code + ResetToken (for testing)
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy tài khoản." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest(new { message = "Token, Code và NewPassword là bắt buộc." });

        try
        {
            var ok = await _authService.ResetPasswordWithCodeAsync(request.Token, request.Code, request.NewPassword, request.ConfirmPassword);
            if (!ok) return BadRequest(new { message = "Token/Code không hợp lệ hoặc hết hạn." });
            return Ok(new { message = "Mật khẩu đã được đặt lại thành công." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Với JWT stateless, việc "logout" chủ yếu là client xóa token
        // Nếu bạn lưu token trong Session thì xóa ở đây
        HttpContext.Session.Remove("JWToken");

        return Ok(new { message = "Đăng xuất thành công. Token đã bị xóa khỏi session." });
    }

}
