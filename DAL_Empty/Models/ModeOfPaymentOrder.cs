using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public class ModeOfPaymentOrder
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "OrderId không được để trống.")]
        [ForeignKey("Order")]
        public Guid? OrderId { get; set; }
        [Required(ErrorMessage = "ModeOfPaymentId không được để trống.")]
        [ForeignKey("ModeOfPayment")]
        public Guid? ModeOfPaymentId { get; set; }

        public virtual ModeOfPayment? ModeOfPayment { get; set; }

        public virtual OrderInfo? Order { get; set; }
    }
}
