using API.Domain.DTOs;
using API.Domain.Request.ColorRequest;

namespace API.Domain.Service.IService
{
    public interface IColorService
    {
        Task<List<ColorDto>> GetAllAsync();
        Task<ColorDto?> GetByIdAsync(Guid id);
        Task<ColorDto> CreateAsync(CreateColorRequest request);
        Task<ColorDto> UpdateAsync(UpdateColorRequest request);
    }
}

