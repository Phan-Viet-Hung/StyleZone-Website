using API.Domain.DTOs;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.Service
{
    public class RoleService : IRoleService
    {
        private readonly DbContextApp _context;
        public RoleService()
        {
            _context = new DbContextApp();
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Select(a => new RoleDto
                {
                    Id = a.Id,
                    Name = a.Name,
                })
                .ToListAsync();
        }

        public async Task<RoleDto> GetRoleByIdAsync(Guid id)
        {
            var a=await _context.Roles.FindAsync(id);
            if (a == null)
                return null;
            return new RoleDto { Id = a.Id,
            Name=a.Name};
        }
    }
}
