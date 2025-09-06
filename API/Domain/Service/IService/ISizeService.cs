using API.Domain.DTOs;

namespace API.Domain.Service.IService
{
    public interface ISizeService
    {
        Task<List<SizeDto>> GetAllAsync();
        Task<SizeDto?> GetByIdAsync(Guid id);
        Task<SizeDto> CreateAsync(string code, string name);
        Task<SizeDto> UpdateAsync(Guid id, string code, string name);
    }
}
