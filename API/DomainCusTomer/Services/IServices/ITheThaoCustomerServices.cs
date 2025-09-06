using API.DomainCusTomer.DTOs;
using API.DomainCusTomer.DTOs.TheThao;
using API.DomainCusTomer.Request.TheThao;

namespace API.DomainCusTomer.Services.IServices
{
    public interface ITheThaoCustomerServices
    {
        Task<PagePickleball> GetALLPICKLEBALL(PickleballFilterRequest filter);
        Task<PagaChayBo> GetAllChayBo(ChayBoFilterRequest filter);
        Task<PageTapLuyen> GetAllTapLuyen(TapLuyenFilterRequest filter);
        Task<PageBongRo> GetAllBongRo(BongRoFilterRequest filter);

        Task<PageCauLong> GetAllCauLong(CauLongFilterRequest filter);
        Task<PageBongDa> GetAllBongDa( BongDaFilterRequest filter);
        Task<PageGolf> GetAllGolf(Golf filter);
        Task<PagedProductResponse> TheThao(ProductFilterRequest filter);
        Task<ProductDetailCustomerDto> GetId(Guid id);
    }
}
