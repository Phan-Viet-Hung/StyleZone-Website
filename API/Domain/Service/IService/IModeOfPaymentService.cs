using API.Domain.DTOs;

namespace API.Domain.Service.IService
{
    public interface IModeOfPaymentService
    {
        Task<IEnumerable<ModeOfPaymentDto>> GetAllAsync();
    }
}
