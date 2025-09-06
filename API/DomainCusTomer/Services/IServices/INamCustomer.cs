using API.DomainCusTomer.DTOs;
using API.DomainCusTomer.DTOs.Nam;
using API.DomainCusTomer.Request;
using API.DomainCusTomer.Request.Nam;

namespace API.DomainCusTomer.Services.IServices
{
    public interface INamCustomer
    {
        Task<PageDoNamCustomer> DoNamCustomer(DoNamCustomerFilterRequest filter);
        Task<PageAonam> AoNam(AoNamFilterRequest filter);
        Task<PageAoTShirtNam> AoTShirtNam(AoTShirtNamFilterRequest filter);
        Task<PageAoPoLoNam> AoPoLoNam(AoPoLoNamFilterRequest filter);
        Task<PageAoGioNam> AoGioNam(AoGioNamFilterRequest filter);
        Task<PageAoNiNam> AoNiNam(AoNiNamFilterRequest filter);
        Task<PageAoDaiTayNam> AoDaiTayNam(AoDaiTayNamFilterRequest filter);
        Task<PageAoLongVuNam> AoLongVuNam(AoLongVuNamFilterRequest filter);
        Task<PageQuanNam> QuanNam(QuanNamFilterRequest filter);
        Task<PageQuanShortNam> QuanShortNam(QuanShortNamFilterRequest filter);
        Task<PageQuanGioNam> QuanGioNam(QuanGioNamFilterRequest filter);
        Task<PageQuanNiNam> QuanNiNam(QuanNiNamFilterRequest filter);
        Task<PageGiayNam> GiayNam(GiayNamFilterRequest filter);
        Task<PageGiayThoiTrangNam> GiayThoiTrangNam(GiayThoiTrangNamFilterRequest filter);
        Task<PageGiayChayBoNam> GiayChayBoNam(GiayChayBoNamFilterRequest filter);
        Task<PageGiayCauLongNam> GiayCauLongNam(GiayCauLongNamFilterRequest filter);
        Task<PageGiayBongRoNam> GiayBongRoNam(GiayBongRoNamFilterRequest filter);
        Task<PageGiayBongDaNam> GiayBongDaNam(GiayBongDaNamFilterRequest filter);
        Task<PageBoQuanAoNam> BoQuanAoNam(BoQuanAoNamFilterRequest filter);
        Task<PageBoQuanAoBongRoNam> BoQuanAoBongRoNam(BoQuanAoBongRoNamFilterRequest filter);
        Task<PageBoQuanAoCauLongNam> BoQuanAoCauLongNam(BoQuanAoCauLongNamFilterRequest filter);
        Task<ProductDetailCustomerDto> GetId(Guid id);
    }
}
