using API.Domain.DTOs;
using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/orderapi/{orderId}/history")]
    public class OrderHistoryController : ControllerBase
    {
        private readonly IOrderHistoryService _orderHistoryService;

        public OrderHistoryController(IOrderHistoryService orderHistoryService)
        {
            _orderHistoryService = orderHistoryService;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderHistoryDto>>> GetByOrderId(Guid orderId)
        {
            var histories = await _orderHistoryService.GetByOrderIdAsync(orderId);
            if (histories == null || !histories.Any())
                return NotFound(new { Message = "Không có lịch sử cho đơn hàng này." });

            return Ok(histories);
        }
    }

}
