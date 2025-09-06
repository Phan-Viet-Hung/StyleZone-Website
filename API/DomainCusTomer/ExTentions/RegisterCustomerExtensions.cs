using API.DomainCusTomer.Config;
using API.DomainCusTomer.DTOs.AccountCustomer;
using DAL_Empty.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace API.DomainCusTomer.ExTentions
{
    public static class RegisterCustomerExtensions
    {
        public static  RegisterCustomerDto ToRegisterDto(this Customer user)
        {
            return new RegisterCustomerDto
            {
                Id = user.Id,
                Email = user.Email.MaskEmail(),               
                Password = user.Password.Mask(),          
                PhoneNumber = user.PhoneNumber.Mask(),      
                Name = user.Fullname,
                UserName = user.UserName.Mask(),
            };
        }
    }
}
