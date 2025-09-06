using System.ComponentModel.DataAnnotations;

namespace DAL_Empty.Models
{
    public class ModeOfPayment
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Tên phương thức thanh toán không được để trống.")]
        [MaxLength(100, ErrorMessage = "Tên phương thức thanh toán không được vượt quá 100 ký tự.")]
        public string? Name { get; set; }
        [MaxLength(100, ErrorMessage = "Tên người tạo không được vượt quá 100 ký tự.")]
        public string? Creator { get; set; }
        [MaxLength(100, ErrorMessage = "Tên người sửa không được vượt quá 100 ký tự.")]

        public string? Fixer { get; set; }
        [Required(ErrorMessage = "Trạng thái không được để trống.")]

        public PaymentStatusEnum Status { get; set; }

        public DateTime? CreationDate { get; set; }

        public DateTime? EditDate { get; set; }

        public virtual ICollection<ModeOfPaymentOrder> ModeOfPaymentOrders { get; set; } = new List<ModeOfPaymentOrder>();
    }
    public enum PaymentStatusEnum
    {
        Active = 1,     // Đang hoạt động
        Inactive = 2,   // Không hoạt động
        Maintenance = 3 // Bảo trì
    }
}
