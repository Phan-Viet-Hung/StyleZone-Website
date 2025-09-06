using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DAL_Empty.Models
{
    public class Address
    {
        [Key]
       
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tỉnh/Thành phố là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tỉnh/Thành phố không được vượt quá 100 ký tự.")]
        public string? Province { get; set; }

        [Required(ErrorMessage = "Quận/Huyện là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Quận/Huyện không được vượt quá 100 ký tự.")]
        public string? District { get; set; }

        [Required(ErrorMessage = "Phường/Xã là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Phường/Xã không được vượt quá 100 ký tự.")]
        public string? Ward { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 03, 05, 07, 08 hoặc 09 và có đúng 10 số.")]
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Địa chỉ chi tiết là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Địa chỉ chi tiết không được vượt quá 200 ký tự.")]
        public string? DetailAddress { get; set; }

        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string? FullName { get; set; }
        [Required(ErrorMessage = "ID khách hàng là bắt buộc.")]
        public Guid? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        [JsonIgnore]
        public virtual Customer? Customer { get; set; }

        [StringLength(50, ErrorMessage = "Trạng thái không được vượt quá 50 ký tự.")]
        public string? Status { get; set; }
    }
}
