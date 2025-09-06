using API.Domain.DTOs;
using API.Domain.Request.SupplierRequest;
using DAL_Empty.Models;

namespace API.Services
{
    public interface ISupplierService
    {
        Task<List<SupplierDto>> GetAllAsync();
        Task<SupplierDto?> GetByIdAsync(Guid id);
        Task<SupplierDto> CreateAsync(CreateSupplierRequest request);
        Task<bool> UpdateAsync(UpdateSupplierRequest request);
    }
}
