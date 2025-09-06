using API.DomainCusTomer.DTOs.TrangChu;

namespace API.DomainCusTomer.Services.IServices
{
    public interface ITrangChuCustomerService
    {
        Task<Dictionary<string, List<HomeProductCustomerDto>>> GetSanPhamTrangChu();
    }
}
