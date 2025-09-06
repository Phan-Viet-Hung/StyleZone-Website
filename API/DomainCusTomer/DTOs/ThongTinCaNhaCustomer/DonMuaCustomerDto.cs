using DAL_Empty.Models;

namespace API.DomainCusTomer.DTOs.ThongTinCaNhaCustomer
{
    public class DonMuaCustomerDto
    {
        public Guid Id { get; set; }
        public OrderStatus Status { get; set; }
        public decimal? TotalAmount { get; set; }
        public List<OrderDetail> OrderDetail { get; set; } = new();
    }
}
