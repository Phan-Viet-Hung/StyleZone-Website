using API.Domain.DTOs;

namespace API.Domain.Service.IService
{
    public interface IPaymentMethodService
    {
        Task<List<PaymentMethodDto>> GetAllAsync();
    }
}
