using API.Domain.DTOs;
using DAL_Empty.Models;

namespace API.Domain.Extentions
{
    public static class SizeExtentions
    {
        public static SizeDto ToDto(this Size s)
        {
            return new SizeDto
            {
                Id = s.Id,
                Code = s.Code ?? string.Empty,
                Name = s.Name ?? string.Empty,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            };
        }
    }
}
