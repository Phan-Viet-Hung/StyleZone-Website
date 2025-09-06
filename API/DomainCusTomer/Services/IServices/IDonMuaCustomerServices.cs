using API.DomainCusTomer.DTOs.QuanLyDonHangCustomerDto;
using API.DomainCusTomer.DTOs.ThongTinCaNhaCustomer;
using API.DomainCusTomer.ExTentions;
using API.DomainCusTomer.Request.ThongTinCaNhan;
using DAL_Empty.Models;

namespace API.DomainCusTomer.Services.IServices
{
    public interface IDonMuaCustomerServices
    {
        Task<List<DonMuaCustomerDto>> GetOrders(string username, OrderStatus? status = null);
        Task<UpdateThongTinCaNhanDto> UpdateCustome(string username, ThongTinCaNhanRequest customer);
        Task<UpdateThongTinCaNhanDto> DetailsCustome(string username);
        Task AddDiaChi(string username, DiachiCustomerDto newAddress);
        Task UpdateDiaChi(Guid Id, DiachiCustomerDto address);
        Task RemoveDiaChi(Guid id);
        Task UpdateStastusDiaChi(Guid Id, string username);

        Task<List<Address>> ListDiaChiCustomer(string username);
        Task UpdatePassWord(RePassDtoCustomer rePassDtoCustomer, string username);


        Task<List<QuanLyDonHangCustomerDto>> ListDonHang(string username);
        Task<List<QuanLyDonHangCustomerDto>> ListDonHangPending(string username);
        Task<List<QuanLyDonHangCustomerDto>> ListDonHangConfirmed(string username);
        Task<List<QuanLyDonHangCustomerDto>> ListDonHangProcessing(string username);

        Task<List<QuanLyDonHangCustomerDto>> ListDonHangShipping(string username);
        Task<List<QuanLyDonHangCustomerDto>> ListDonHangDelivered(string username);

        Task<List<QuanLyDonHangCustomerDto>> ListDonHangCancelled(string username);

        public Task CancelOrderAsync(Guid orderId, string username, string Decription);
        public Task CancelOrderAsyncGuest(Guid orderId, string Decription);

    }
}
