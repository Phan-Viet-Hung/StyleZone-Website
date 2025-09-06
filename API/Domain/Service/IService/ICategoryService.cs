using API.Domain.DTOs;
using API.Domain.Request.CategoryRequest;

namespace API.Domain.Service.IService
{
    public interface ICategoryService
    {
        Task<CategoryDto> CreateAsync(CreateCategoryRequest request);
        Task<CategoryDto> UpdateAsync(UpdateCategoryRequest request);
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(Guid id);
    }
}
