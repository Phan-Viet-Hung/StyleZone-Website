using System.ComponentModel.DataAnnotations;
using API.Domain.Request.AccountRequest;
using DAL_Empty.Models;

namespace API.Request
{
    public class UpdateAccountRequest
    {
        [Required]
        public Guid Id { get; set; }

        public string Name { get; set; }
        
        public DateTime? Birthday { get; set; }
       
        public string? Email { get; set; }
        
        public string? PhoneNumber { get; set; }

        public GenderEnum? Gender { get; set; }

        [Required]
        public string UserName { get; set; }
        public string? Password { get; set; }
        public bool IsActive { get; set; }


        public string? Address { get; set; }

        public Guid? RoleId { get; set; }
    }
}
