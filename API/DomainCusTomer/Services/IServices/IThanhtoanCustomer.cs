using API.DomainCusTomer.DTOs.QuanLyDonHangCustomerDto;
using API.DomainCusTomer.DTOs.ThanhToanCustomer;

namespace API.DomainCusTomer.Services.IServices
{
    public interface IThanhtoanCustomer
    {
        Task<OrderID> CreateGuestOrderAsync(OrderGuestDto request);
        Task<QuanLyDonHangCustomerDto> Tracuudonhang(string orderid);
    }
}
