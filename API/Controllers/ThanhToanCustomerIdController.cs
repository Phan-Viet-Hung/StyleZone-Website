
using API.DomainCusTomer.DTOs.ThanhToanCustomerId;
using API.DomainCusTomer.Request.MuaNgay;
using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThanhToanCustomerIdController : Controller
    {
        private readonly IThanhtoanCartIdServices _thanhtoanCustomer;
        public ThanhToanCustomerIdController(IThanhtoanCartIdServices thanhtoanCustomer)
        {
            _thanhtoanCustomer = thanhtoanCustomer;
        }
        [HttpPost("create-by-customer-id")]
        public async Task<IActionResult> CreateOrderByCustomerId([FromBody] OrderCustomerIdDto request, string username)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var orderId = await _thanhtoanCustomer.CreateOrderAsyncCustomerid(request, username);
                return Ok(orderId);
            }
            catch (InvalidOperationException ex)
            {
                // Lỗi do business logic: sản phẩm hết hàng, voucher không hợp lệ,...
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Lỗi hệ thống khác
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống khi tạo đơn hàng." });
            }
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetCartViewModel(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest(new { message = "Username không được để trống" });

            try
            {
                var result = await _thanhtoanCustomer.GetCartViewModelAsync(username);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy giỏ hàng", error = ex.Message });
            }
        }
        [HttpPost("addmua-ngay")]
        public async Task<IActionResult> MuaNgay([FromBody] MuaNgayCustomerRequest request, string username)
        {
            if (request == null || string.IsNullOrEmpty(request.ProductDetailcodeMuaNgay))
                return BadRequest(new { message = "Yêu cầu không hợp lệ" });

            try
            {
                var result = await _thanhtoanCustomer.MuaNgayAddAsync(HttpContext, request, username);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi thêm sản phẩm vào giỏ hàng", error = ex.Message });
            }
        }

        [HttpGet("currentmua-ngay")]
        public async Task<IActionResult> GetMuaNgay(string username)
        {
            try
            {
                var item = await _thanhtoanCustomer.MuaNgayAsync(HttpContext, username);
                if (item == null)
                    return NotFound(new { message = "Không có sản phẩm mua ngay" });

                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Đã xảy ra lỗi khi lấy sản phẩm mua ngay", error = ex.Message });
            }
        }
        [HttpDelete("remove-all/{username}")]
        public async Task<IActionResult> RemoveAllItems(string username)
        {
            try
            {
                await _thanhtoanCustomer.RemoveCartItem(username);
                return Ok(new
                {
                    message = "Đã xóa toàn bộ sản phẩm trong giỏ hàng"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }

    }
}
