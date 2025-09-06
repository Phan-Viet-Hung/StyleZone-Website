using DAL_Empty.Models;

namespace API.Domain.Request.OrderRequest
{
    public class BulkUpdateOrderStatusRequest
    {
        public List<Guid> OrderIds { get; set; } = new();
        public OrderStatus Status { get; set; }
    }
}
