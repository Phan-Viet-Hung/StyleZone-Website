using MVC.Models.Cart;


namespace MVC.Services
{
    public interface ICartCustomerMVCService
    {
        Task<List<CartCustomerMVCDto>> GetCurrentAsync(HttpContext ctx);
        Task<List<CartCustomerMVCDto>> AddAsync(HttpContext ctx, CartCustomerMVCRequest cartCustomerRequest);
        Task UpdateQtyAsync(HttpContext ctx, CartCustomerMVCRequest cartCustomerRequest);
        Task RemoveAsync(HttpContext ctx, string ProductCode);
    }
}
