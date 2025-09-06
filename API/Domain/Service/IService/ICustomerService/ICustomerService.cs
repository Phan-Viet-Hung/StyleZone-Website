using API.Domain.DTOs;
using API.Domain.Request.CustomerRequest;

namespace API.Domain.Service.IService.ICustomerService
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<CustomerDto?> GetByIdAsync(Guid id);
        Task<bool> UpdateStatusAsync(Guid id, string status);
        Task<int> UpdateStatusBulkAsync(List<Guid> ids, string status);
    }
}
