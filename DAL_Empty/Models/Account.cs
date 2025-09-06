using DAL_Empty.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace DAL_Empty.Models
{
    public enum GenderEnum
    {
        Nam = 1,
        Nu = 2,
        Khac = 3
    }
    public class Account
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [MaxLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string? Name { get; set; }
        [MinAge(16, ErrorMessage = "Bạn phải đủ ít nhất 16 tuổi.")]

        public DateTime Birthday { get; set; }

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [MaxLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại không đúng định dạng Việt Nam.")]
        [MaxLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [MaxLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự.")]
        public string? UserName { get; set; }

        public GenderEnum? Gender { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Password { get; set; }

        [MaxLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự.")]
        public string? Address { get; set; }
        public bool IsActive { get; set; } = false;

        public Guid? RoleId { get; set; }

        public virtual ICollection<OrderInfo> OrderInfos { get; set; } = new List<OrderInfo>();

        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }
    }
}
