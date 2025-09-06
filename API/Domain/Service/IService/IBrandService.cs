using API.Domain.DTOs;
using API.Domain.Request.BrandRequest;

namespace API.Domain.Service.IService
{
    public interface IBrandService
    {
        Task<List<BrandDto>> GetAllAsync();
        Task<BrandDto?> GetByIdAsync(Guid id);
        Task<BrandDto> CreateAsync(CreateBrandRequest request);
        Task<BrandDto> UpdateAsync(Guid id, UpdateBrandRequest request);
    }
}
