using DAL_Empty.Models;

namespace API.Domain.DTOs.ThongKe
{
    public class OrderStatusStatisticDto
    {
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; }
        public int TotalOrders { get; set; }
    }
}
