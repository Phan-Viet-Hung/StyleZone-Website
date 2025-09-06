using API.DomainCusTomer.DTOs.CastCustomerId;
using API.DomainCusTomer.DTOs.MuangayCustomer;
using API.DomainCusTomer.DTOs.MuaNgayCustomerID;
using API.DomainCusTomer.Request.Cast;
using API.DomainCusTomer.Request.MuaNgay;
using DAL_Empty.Models;

namespace API.DomainCusTomer.Services.IServices
{
    public interface ICartCustomerIDServices
    {
        Task<List<CastCustomerIDDto>> GetCurrenIDtAsync(string username);
        Task AddIDAsync(string username, CartCustomerRequest cartCustomerRequest);
        Task AddListAsync(string username, List<CartCustomerRequest> requests);
        Task RemoveIDAsync(Guid Id );
        Task UpdateIDIncreaseAsync(Guid Id);
        Task UpdateIDReduceAsync(Guid Id );
        //Task<MuangaycustomerIdDto> MuaNgayAsync(HttpContext ctx);
        //Task<MuangaycustomerIdDto> MuaNgayAddAsync(HttpContext ctx, MuaNgayCustomerRequest muaNgayCustomerRequest, string username);
        //Task MuaNgayCustomerID(string Productcode);
        Task<List<string>> ValidateIDCartQuantityAsync(string username);
    }
}
