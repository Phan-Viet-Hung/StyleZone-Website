using API.DomainCusTomer.Request.Cast;
using API.DomainCusTomer.Request.MuaNgay;
using API.DomainCusTomer.Services;
using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartCustomerIDController : ControllerBase
    {
        private readonly ICartCustomerIDServices _services;
        public CartCustomerIDController(ICartCustomerIDServices services) 
        {
            _services = services;
        }
        [HttpGet("validate-quantity")]
        public async Task<IActionResult> ValidateQuantity(string username)
        {
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Bạn cần đăng nhập để thực hiện thao tác này." });

            var errors = await _services.ValidateIDCartQuantityAsync(username);

            if (errors.Any())
                return BadRequest(new { errors });
            return Ok(new { message = "Số lượng sản phẩm hợp lệ." });
        }
        [HttpGet("{username}")]
        public async Task<IActionResult> GetCartByUsername(string username)
        {
            try
            {
                var cartItems = await _services.GetCurrenIDtAsync(username);

                if (cartItems == null || !cartItems.Any())
                {
                    return NotFound(new { message = "Không có sản phẩm nào trong giỏ hàng" });
                }

                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy giỏ hàng.", error = ex.Message });
            }
        }
        [HttpPost("{username}")]
        public async Task<IActionResult> AddToCart(string username, [FromBody] CartCustomerRequest request)
        {
            try
            {
                await _services.AddIDAsync(username, request);
                return Ok(new { message = "Thêm sản phẩm vào giỏ hàng thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi thêm sản phẩm vào giỏ hàng.", error = ex.Message });
            }
        }
        [HttpPost("merge/{username}")]
        public async Task<IActionResult> MergeCart(string username, [FromBody] List<CartCustomerRequest> requests)
        {
            await _services.AddListAsync(username, requests);
            return Ok(new { message = "Merge cart thành công" });
        }
        [HttpDelete("{Id}")]
        public async Task<IActionResult> RemoveFromCart(Guid Id)
        {
            try
            {
                await _services.RemoveIDAsync(Id);
                return Ok(new { message = "Xóa sản phẩm khỏi giỏ hàng thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi xóa sản phẩm khỏi giỏ hàng.", error = ex.Message });
            }
        }

        // 🔼 Tăng số lượng sản phẩm trong giỏ hàng
        [HttpPost("increase/{Id}")]
        public async Task<IActionResult> IncreaseQuantity(Guid Id)
        {
            try
            {
                await _services.UpdateIDIncreaseAsync(Id);
                return Ok(new { message = "Tăng số lượng sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi tăng số lượng sản phẩm.", error = ex.Message });
            }
        }

        // 🔽 Giảm số lượng sản phẩm trong giỏ hàng
        [HttpPost("decrease/{Id}")]
        public async Task<IActionResult> DecreaseQuantity(Guid Id)
        {
            try
            {
                await _services.UpdateIDReduceAsync(Id);
                return Ok(new { message = "Giảm số lượng sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi giảm số lượng sản phẩm.", error = ex.Message });
            }
        }
        //[HttpPost("addmua-ngay")]
        //public async Task<IActionResult> MuaNgay([FromBody] MuaNgayCustomerRequest request)
        //{
        //    if (request == null || string.IsNullOrEmpty(request.ProductDetailcodeMuaNgay))
        //        return BadRequest("Yêu cầu không hợp lệ");

        //    try
        //    {
        //        var result = await _services.MuaNgayAddAsync(HttpContext, request);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
        //[HttpGet("currentmua-ngay")]
        //public async Task<IActionResult> GetMuaNgay()
        //{
        //    var item = await _services.MuaNgayAsync(HttpContext);
        //    if (item == null)
        //        return NotFound("Không có sản phẩm mua ngay");
        //    return Ok(item);
        //}
    }
}
