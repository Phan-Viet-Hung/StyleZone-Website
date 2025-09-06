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
    public class CartCustomerController : ControllerBase
    {
        private readonly ICartCustomerService _cartService;
        public CartCustomerController(ICartCustomerService cartService)
        {
            _cartService = cartService;
        }
        [HttpPost("addmua-ngay")]
        public async Task<IActionResult> MuaNgay([FromBody] MuaNgayCustomerRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ProductDetailcodeMuaNgay))
                return BadRequest("Yêu cầu không hợp lệ");

            try
            {
                var result = await _cartService.MuaNgayAddAsync(HttpContext, request);
                return Ok(result); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("currentmua-ngay")]
        public async Task<IActionResult> GetMuaNgay()
        {
            var item = await _cartService.MuaNgayAsync(HttpContext);
            if (item == null)
                return NotFound("Không có sản phẩm mua ngay");
            return Ok(item);
        }
    
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrent()
        {

           await _cartService.GetCurrentAsync(HttpContext);
            return Ok();
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] CartCustomerRequest request)
        {
            var result = await _cartService.AddAsync(HttpContext, request);
            return Ok(result);
        }

        //[HttpPut("update")]
        //public async Task<IActionResult> Update([FromBody] CartCustomerRequest request)
        //{
        //    if (request == null || string.IsNullOrEmpty(request.ProductDetailcode))
        //        return BadRequest("Dữ liệu không hợp lệ");

        //    try
        //    {
        //        await _cartService.UpdateQtyAsync(HttpContext, request);
        //        return Ok("Cập nhật số lượng thành công");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}



        [HttpDelete("clear")]
        public IActionResult ClearCart()
        {
            var ctx = HttpContext;
            ctx.Response.Cookies.Delete("CustomerCart");

            return Ok();
        }
        // ================== 4. Xóa sản phẩm ==================
        [HttpGet("Remove")]
        public async Task<IActionResult> Remove(string ProductDetailcode)
        {
            if (string.IsNullOrEmpty(ProductDetailcode))
                return BadRequest("Mã sản phẩm không hợp lệ");

            try
            {
                await _cartService.RemoveAsync(HttpContext, ProductDetailcode);
                return Ok("Xóa sản phẩm khỏi giỏ hàng thành công");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[HttpGet("increase")]
        //public async Task<IActionResult> Increase(string ProductDetailcode)
        //{
        //    if (string.IsNullOrEmpty(ProductDetailcode))
        //        return BadRequest("Mã sản phẩm không hợp lệ");

        //    try
        //    {
        //        var updatedCart = await _cartService.Updateincrease(HttpContext, ProductDetailcode);
        //        return Ok(updatedCart); 
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpGet("increase")]
        public async Task<IActionResult> Increase(string ProductDetailcode)
        {
            if (string.IsNullOrEmpty(ProductDetailcode))
                return BadRequest("Mã sản phẩm không hợp lệ");

            try
            {
                await _cartService.Updateincrease(HttpContext, ProductDetailcode);
                return Ok("Update sản phẩm trong giỏ hàng thành công");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // --------- Giảm số lượng ----------
        [HttpGet("reduce")]
        public async Task<IActionResult> Reduce(string ProductDetailcode)
        {
            if (string.IsNullOrEmpty(ProductDetailcode))
                return BadRequest("Mã sản phẩm không hợp lệ");
            try
            {
                await _cartService.Updatereduce(HttpContext, ProductDetailcode);
                return Ok("Update sản phẩm trong giỏ hàng thành công");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("validate")]
        public async Task<IActionResult> ValidateCartBeforeCheckout()
        {
            try
            {
                var errors = await _cartService.ValidateCartQuantityAsync(HttpContext);
                if (errors.Any())
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Errors = errors
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Tất cả sản phẩm trong giỏ hàng hợp lệ."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi kiểm tra giỏ hàng: {ex.Message}");
            }
        }



    }
}
