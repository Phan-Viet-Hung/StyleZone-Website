    using API.DomainCusTomer.DTOs.MuaNgayCustomerID;
using API.DomainCusTomer.DTOs.ThanhToanCustomer;
using API.DomainCusTomer.DTOs.ThanhToanCustomerId;
using API.DomainCusTomer.Request.MuaNgay;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace API.DomainCusTomer.Services.IServices
{
    public interface IThanhtoanCartIdServices
    {
        Task<ThanhToanCartIdDto> GetCartViewModelAsync(string username);
        Task<MuangaycustomerIdDto> MuaNgayAsync(HttpContext ctx, string username);
        Task<MuangaycustomerIdDto> MuaNgayAddAsync(HttpContext ctx, MuaNgayCustomerRequest muaNgayCustomerRequest, string username);
        Task<OrderID> CreateOrderAsyncCustomerid(OrderCustomerIdDto request, string username);
        Task RemoveCartItem(string username);
    }
}
