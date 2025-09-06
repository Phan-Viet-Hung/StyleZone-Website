using DAL_Empty.Models;

namespace API.Domain.Request.OrderRequest
{
    public class UpdateOrderStatusRequest
    {
        public Guid OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public string? Reason { get; set; }
    }
}
