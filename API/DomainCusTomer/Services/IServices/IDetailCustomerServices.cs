using API.DomainCusTomer.DTOs;
using API.DomainCusTomer.DTOs.DetailCustomer;
using API.DomainCusTomer.ExTentions;

namespace API.DomainCusTomer.Services.IServices
{
    public interface IDetailCustomerServices
    {
        Task<DetailCustomerDto> GetId(Guid id);
    }
}
