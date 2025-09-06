using API.DomainCusTomer.DTOs.CartICustomer;
using API.DomainCusTomer.DTOs.MuangayCustomer;
using API.DomainCusTomer.Request.Cast;
using API.DomainCusTomer.Request.MuaNgay;
using DAL_Empty.Models;

namespace API.DomainCusTomer.Services.IServices
{
    public interface ICartCustomerService
    {
        Task<List<CartCustomerDto>> GetCurrentAsync(HttpContext ctx);
        Task<List<CartCustomerDto>> AddAsync(HttpContext ctx, CartCustomerRequest cartCustomerRequest);
        Task RemoveAsync(HttpContext ctx, string ProductDetailcode);
        Task Updateincrease(HttpContext ctx, string ProductDetailcode);
        Task Updatereduce(HttpContext ctx, string ProductDetailcode );
        Task<MuangaycustomerDto> MuaNgayAsync(HttpContext ctx);
        Task<MuangaycustomerDto> MuaNgayAddAsync(HttpContext ctx, MuaNgayCustomerRequest muaNgayCustomerRequest);
        Task<List<string>> ValidateCartQuantityAsync(HttpContext ctx);
        //Task<List<CartCustomerDto>> Updateincrease(HttpContext ctx, string ProductDetailcode);
        //Task UpdateQtyAsync(HttpContext ctx, CartCustomerRequest cartCustomerRequest);
    }
}
