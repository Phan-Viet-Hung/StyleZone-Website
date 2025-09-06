using API.DomainCusTomer.DTOs.ThanhToanCustomer;
using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThanhToanCustomerController : ControllerBase
    {
        private readonly IThanhtoanCustomer _thanhtoanCustomer;
        public ThanhToanCustomerController(IThanhtoanCustomer thanhtoanCustomer)
        {
            _thanhtoanCustomer = thanhtoanCustomer;
        }

        [HttpPost("create-guest-order")]
        public async Task<IActionResult> CreateGuestOrder([FromBody] OrderGuestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var orderId = await _thanhtoanCustomer.CreateGuestOrderAsync(request);

                return Ok(orderId);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Lỗi hệ thống khác
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống khi tạo đơn hàng." });
            }
        }
        [HttpGet("tracuudonhang/{orderid}")]
        public async Task<IActionResult> Tracuudonhang(string orderid)
        {
            try
            {
                var result = await _thanhtoanCustomer.Tracuudonhang(orderid);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Lỗi hệ thống khác
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống khi tạo đơn hàng." });
            }
        }

    }
}