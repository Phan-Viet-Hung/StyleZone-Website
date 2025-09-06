using API.DomainCusTomer.DTOs;
using API.DomainCusTomer.DTOs.ThoiTrang;
using API.DomainCusTomer.Request;
using API.DomainCusTomer.Request.ThoiTrang;

namespace API.DomainCusTomer.Services.IServices
{
    public interface IThoiTrangCustomerServices
    {
        Task<PageThoiTrang> GetALLThoiTrang(ThoiTrangFilterRequst filter);
        Task<PageWaDe> GetAllWaDe(WadeFilterRequst filter);
        Task<PageBADFIVE> GetAllBADFIVE(BADFIVEFilterRequst filter);
        Task<PageLIFESTYLE> GetAllLIFESTYLE(LIFESTYLEFilterRequst filter);

        Task<PageISAAC> GetAllISAAC(ISAACFilterRequst filter);
        Task<PageYOUNG> GetAllYOUNG(YOUNGFilterRequst filter);
        Task<PageBeTrai> GetAllBeTrai(BeTraiFilterRequst filter);
        Task<PageBeGai> GetAllBeGai(BeGaiFilterRequst filter);
        Task<PageOUTLET> GetAllOUTLET(OUTLETFilterRequst filter);
        Task<PageOUTLETPICKLEBALL> GetAllOUTLETPICKLEBALL(OUTLETPICKLEBALLFilterRequst filter);
        Task<ProductDetailCustomerDto> GetId(Guid id);
    }
}
