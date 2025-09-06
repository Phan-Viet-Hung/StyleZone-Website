using API.DomainCusTomer.DTOs.AccountCustomer;
using DAL_Empty.Models;

namespace API.DomainCusTomer.DTOs.ThanhToanCustomer
{
    public static class OrderIDDto
    {
        public static OrderID ToRegisterDto(this OrderInfo q)
        {
            return new OrderID
            {
                Id = q.Id,
            };
        }

    }
}
