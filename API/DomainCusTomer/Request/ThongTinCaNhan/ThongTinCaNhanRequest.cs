using DAL_Empty.Models;
using DAL_Empty.Validations;
using System.ComponentModel.DataAnnotations;

namespace API.DomainCusTomer.Request.ThongTinCaNhan
{
    public class ThongTinCaNhanRequest
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string? Fullname { get; set; }
        [MinAge(16, ErrorMessage = "Bạn phải đủ ít nhất 16 tuổi.")]
        public DateTime? Birthday { get; set; }

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [MaxLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự.")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 03, 05, 07, 08 hoặc 09 và có đúng 10 số.")]
        public string? PhoneNumber { get; set; }

        [MaxLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự.")]
        public string? UserName { get; set; }

        public GenderEnum? Gender { get; set; }
    }
}
