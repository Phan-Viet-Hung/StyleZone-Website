using API.Domain.Request.OrderRequest;
using DAL_Empty.Models;
using API.Domain.DTOs;

namespace API.Domain.Service.IService
{
    public interface IOrderService
    {
        Task<OrderDto> CreatePosOrderAsync(CreateOrderRequest request, Guid userId);
        Task<List<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(Guid orderId);
        Task<bool> DeleteOrderAsync(Guid orderId);
        Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, Guid updatedBy, string? reason);
        Task<(int updatedCount, List<string> errors)> UpdateOrderStatusBulkAsync(
     List<Guid> orderIds, OrderStatus status, Guid updatedBy);

    }
}
