using API.Domain.DTOs;
using DAL_Empty.Models;

namespace API.Domain.Service.IService
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> GetRoleByIdAsync(Guid id);
    }
}
