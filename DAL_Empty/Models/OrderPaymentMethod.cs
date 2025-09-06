using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public class OrderPaymentMethod
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Order ID là bắt buộc")]
        [ForeignKey("Order")]
        public Guid? OrderId { get; set; }
        [Required(ErrorMessage = "Payment Method ID là bắt buộc")]
        [ForeignKey("PaymentMethod")]
        public Guid? PaymentMethodId { get; set; }
        [Required(ErrorMessage = "Giá không được để trống.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PaymentAmount { get; set; }

        public virtual OrderInfo? Order { get; set; }

        public virtual PaymentMethod? PaymentMethod { get; set; }
    }
}
