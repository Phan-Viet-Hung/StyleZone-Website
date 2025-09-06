using API.DomainCusTomer.DTOs;
using API.DomainCusTomer.DTOs.Nu;
using API.DomainCusTomer.Request;
using API.DomainCusTomer.Request.Nu;

namespace API.DomainCusTomer.Services.IServices
{
    public interface INuCustomer
    {
        Task<PageDoNuCustome> DoNuCustomer(DoNuCustomerFilterRequest filter);
        Task<PageAoNu> AoNu(AoNuFilterRequest filter);
        Task<PageAoTShirtNu> AoTShirtNu(AoTShirtNuFilterRequest filter);
        Task<PageAoPoLoNu> AoPoLoNu(AoPoLoNuFilterRequest filter);
        Task<PageAoGioNu> AoGioNu(AoGioNuFilterRequest filter);
        Task<PageAoNiNu> AoNiNu(AoNiNuFilterRequest filter);
        Task<PageAoDaiTayNu> AoDaiTayNu(AoDaiTayNuFilterRequest filter);
        Task<PageAoLongVuNu> AoLongVuNu(AoLongVuNuFilterRequest filter);
        Task<PageQuanNu> QuanNu(QuanNuFilterRequest filter);
        Task<PageQuanShortNu> QuanShortNu(QuanShortNuFilterRequest filter);
        Task<PageQuanGioNu> QuanGioNu(QuanGioNuFilterRequest filter);
        Task<PageQuanNiNu> QuanNiNu(QuanNiNuFilterRequest filter);
        Task<PageGiayNu> GiayNu(GiayNuFilterRequest filter);
        Task<PageGiayThoiTrangNu> GiayThoiTrangNu(GiayThoiTrangNuFilterRequest filter);
        Task<PageGiayChayBoNu> GiayChayBoNu(GiayChayBoNuFilterRequest filter);
        Task<PageGiayCauLongNu> GiayCauLongNu(GiayCauLongNuFilterRequest filter);
        Task<PageGiayBongRoNu> GiayBongRoNu(GiayBongRoNuFilterRequest filter);
        Task<PageGiayBongDaNu> GiayBongDaNu(GiayBongDaNuFilterRequest filter);
        Task<PageBoQuanAoNu> BoQuanAoNu(BoQuanAoNuFilterRequest filter);
        Task<PageBoQuanAoBongRoNu> BoQuanAoBongRoNu(BoQuanAoBongRoNuFilterRequest filter);
        Task<PageBoQuanAoCauLongNu> BoQuanAoCauLongNu(BoQuanAoCauLongNuFilterRequest filter);
        Task<ProductDetailCustomerDto> GetId(Guid id);
    }
}
