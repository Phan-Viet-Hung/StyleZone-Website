using API.DomainCusTomer.DTOs.Tintuc;

namespace API.DomainCusTomer.Services.IServices
{
    public interface ITinTucService
    {
        Task<List<TinTucDto>> GetAllTinTucAsync();
        Task<TinTucDetailDto> GetTinTucByIdAsync(Guid id);
    }
}
