using API.DomainCusTomer.DTOs.AccountCustomer;
using API.DomainCusTomer.ExTentions;
using API.DomainCusTomer.Request.AccountCustomerRequest;
using API.DomainCusTomer.Request.LoginAccountCustomerRequest;
using DAL_Empty.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace API.DomainCusTomer.Services.IServices
{
    public interface ILoginAccountCustomerServices
    {
        Task<RegisterCustomerDto> RegisterAsync(RegisteCustomerRequest registeRequest);
        Task<Customer?> LoginAsync(LoginnCustomerRequest loginRequest);
        Task<Customer> forgotpassword(ForgotpasswordCustomerRequest registeRequest);
        bool CheckEmail(string email);
        Task<Customer> LoginGoole(LoginGoogleCustomerRequest registeRequest);
        Task GetByUsernameAsync(string username);
    }
}
