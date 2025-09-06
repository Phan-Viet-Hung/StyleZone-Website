using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public class PaymentMethod
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Tên phương thức thanh toán là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên phương thức thanh toán không được vượt quá 100 ký tự")]
        public string? Name { get; set; }
        public virtual ICollection<OrderPaymentMethod> OrderPaymentMethods { get; set; } = new List<OrderPaymentMethod>();
    }
}
