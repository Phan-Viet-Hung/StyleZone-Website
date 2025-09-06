using API.DomainCusTomer.DTOs.ThongTinCaNhaCustomer;
using DAL_Empty.Models;

namespace API.DomainCusTomer.ExTentions
{
    public static class DonMuaCustomerExtention
    {
        public static DonMuaCustomerDto ToDonMuaCustomerDto(this OrderInfo p)
        {
            return new DonMuaCustomerDto
            {
                Id = p.Id,
                Status = p.Status,
                TotalAmount = p.TotalAmount,
                OrderDetail = p.OrderDetails?.ToList() ?? new List<OrderDetail>()
            };
        }
    }
}
