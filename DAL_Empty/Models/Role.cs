using System.ComponentModel.DataAnnotations;

namespace DAL_Empty.Models
{
    public class Role
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên vai trò không được để trống.")]
        [MaxLength(50, ErrorMessage = "Tên vai trò không được vượt quá 50 ký tự.")]
        public string? Name { get; set; }

        public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}
