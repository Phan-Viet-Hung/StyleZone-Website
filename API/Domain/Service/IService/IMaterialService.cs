using API.Domain.DTOs;
using API.Domain.Request.MaterialRequest;

namespace API.Domain.Service.IService
{
    public interface IMaterialService
    {
        Task<List<MaterialDto>> GetAllAsync();
        Task<MaterialDto?> GetByIdAsync(Guid id);
        Task<MaterialDto> CreateAsync(CreateMaterialRequest request);
        Task<MaterialDto> UpdateAsync(UpdateMaterialRequest request);
    }
}
