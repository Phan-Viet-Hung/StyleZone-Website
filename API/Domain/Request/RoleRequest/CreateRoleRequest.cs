using System.ComponentModel.DataAnnotations;
using DAL_Empty.Models;

namespace API.Domain.Request.RoleRequest
{
    public class CreateRoleRequest
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên vai trò không được để trống.")]
        [MaxLength(50, ErrorMessage = "Tên vai trò không được vượt quá 50 ký tự.")]
        public string? Name { get; set; }
    }
}
