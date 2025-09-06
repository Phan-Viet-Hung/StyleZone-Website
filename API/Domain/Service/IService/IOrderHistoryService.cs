using API.Domain.DTOs;

namespace API.DomainCusTomer.Services.IServices
{
    public interface IOrderHistoryService
    {
        Task<List<OrderHistoryDto>> GetByOrderIdAsync(Guid orderId);
    }
}
