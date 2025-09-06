using DAL_Empty.Validations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DAL_Empty.Models
{

    public class Customer
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [MaxLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string? Fullname { get; set; }
        [MinAge(16, ErrorMessage = "Bạn phải đủ ít nhất 16 tuổi.")]
        public DateTime? Birthday { get; set; }

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [MaxLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự.")]
        public string? Email { get; set; }
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [MaxLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự.")]
        public string? PhoneNumber { get; set; }

        [MaxLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự.")]
        public string? UserName { get; set; }

        public GenderEnum? Gender { get; set; }

        public DateTime? CreateAt { get; set; }

        public DateTime? UpdateAt { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [MaxLength(255, ErrorMessage = "Mật khẩu không được vượt quá 255 ký tự.")]
        public string? Password { get; set; }
        [Required]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }
         [JsonIgnore]
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        [JsonIgnore]
        public virtual Cart? Cart { get; set; }
        //[JsonIgnore]
        //public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
        [JsonIgnore]
        public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; } = new List<CustomerVoucher>();
        [JsonIgnore]
        public virtual ICollection<OrderInfo> OrderInfos { get; set; } = new List<OrderInfo>();
    }
}
