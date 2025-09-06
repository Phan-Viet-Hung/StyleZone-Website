using API.DomainCusTomer.DTOs.ThongTinCaNhaCustomer;
using DAL_Empty.Models;

namespace API.DomainCusTomer.ExTentions
{
    public static class UpdateThongTinCustomerExtention
    {
        public static UpdateThongTinCaNhanDto ToUpdateThongTinCaNhanDto( this Customer p)
        {
            return new UpdateThongTinCaNhanDto
            {
                Fullname = p.Fullname,
                UserName = p.UserName,
                Birthday = p.Birthday,
                Email = p.Email,
                PhoneNumber = p.PhoneNumber,
                Gender = p.Gender,
            };
        }
    }
}
