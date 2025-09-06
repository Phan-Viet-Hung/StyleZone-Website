using API.Domain.DTOs;
using API.Domain.Request.OriginRequest;

namespace API.Domain.Service.IService
{
public interface IOriginService
{
    Task<List<OriginDto>> GetAllAsync();
    Task<OriginDto?> GetByIdAsync(Guid id);
    Task<OriginDto> CreateAsync(CreateOriginRequest request);
    Task<OriginDto> UpdateAsync(UpdateOriginRequest request);
}
}
