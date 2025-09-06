using Microsoft.AspNetCore.Mvc;
using API.Domain.Service.IService;
using API.Domain.DTOs;
using API.Domain.Request;
using API.Domain.Service.IService;
using API.Domain.Request.OrderRequest;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DAL_Empty.Models;

namespace API.Controllers
{
    [ApiController]
    [Authorize] // ✅ Đảm bảo yêu cầu JWT token
    [Route("api/[controller]")]
    public class OrderApiController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderApiController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/orderapi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // GET: api/orderapi/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            return Ok(order);
        }

        // ✅ Đảm bảo yêu cầu JWT token
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // ✅ Kiểm tra tồn tại claim NameIdentifier (userId)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new
                    {
                        message = "Token không chứa thông tin người dùng (ClaimTypes.NameIdentifier)."
                    });
                }

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new
                    {
                        message = "Claim userId không hợp lệ. Không thể parse thành Guid."
                    });
                }

                // ✅ Nếu hợp lệ, tạo đơn hàng
                var newId = await _orderService.CreatePosOrderAsync(request, userId);
                return CreatedAtAction(nameof(GetById), new { id = newId }, new { id = newId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Lỗi tạo đơn hàng",
                    error = ex.Message
                });
            }
        }
            [HttpPut("update-status")]
            public async Task<IActionResult> UpdateStatus([FromBody] UpdateOrderStatusRequest request)
            {
                if (request == null)
                    return BadRequest(new { message = "Dữ liệu không hợp lệ" });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized(new { message = "Không lấy được thông tin người dùng" });

                try
                {
                    var success = await _orderService.UpdateOrderStatusAsync(
                        request.OrderId,
                        (OrderStatus)request.Status,
                        userId,
                        request.Reason // 👈 truyền lý do xuống service
                    );

                    return Ok(new { message = "Cập nhật trạng thái thành công" });
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }


        [HttpPut("update-status-bulk")]
        public async Task<IActionResult> UpdateStatusBulk([FromBody] BulkUpdateOrderStatusRequest request)
        {
            if (request == null || request.OrderIds == null || !request.OrderIds.Any())
                return BadRequest(new { message = "Danh sách đơn hàng không hợp lệ" });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Không lấy được thông tin người dùng" });

            try
            {
                var updatedCount = await _orderService.UpdateOrderStatusBulkAsync(
                    request.OrderIds,
                    (OrderStatus)request.Status,
                    userId
                );

                return Ok(new { message = $"Đã cập nhật trạng thái cho {updatedCount}/{request.OrderIds.Count} đơn hàng" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }





    }
}