using API.Domain.DTOs;
using DAL_Empty.Models;

namespace API.Domain.Extentions
{
    public static class MaterialExtentions
    {
        public static MaterialDto ToDto(this Material m)
        {
            return new MaterialDto
            {
                Id = m.Id,
                Name = m.Name ?? string.Empty,
                Description = m.Description,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            };
        }
    }
}
