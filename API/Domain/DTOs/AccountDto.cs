using System.ComponentModel.DataAnnotations.Schema;
using DAL_Empty.Models;
namespace API.Domain.DTOs
{
    public class AccountDto
    {

        public Guid Id { get; set; }

        

        public string? Name { get; set; }

        public DateTime? Birthday { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? UserName { get; set; }

        public GenderEnum? Gender { get; set; }

        public bool IsActive { get; set; }
        // sửa từ bool? => bool

        public string? Address { get; set; }

        public string? roleName { get; set; }
        public Guid? RoleId { get; set; }

        
    }
}
