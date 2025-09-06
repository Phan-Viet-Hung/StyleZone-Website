using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public class CustomerVoucher
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime? UsedDate { get; set; }
        [Required(ErrorMessage = "CustomerId không được để trống.")]
        [ForeignKey("Customer")]
        public Guid? CustomerId { get; set; }

        [Required(ErrorMessage = "VoucherId không được để trống.")]
        [ForeignKey("Voucher")]
        public Guid? VoucherId { get; set; }

        public virtual Customer? Customer { get; set; }

        public virtual Voucher? Voucher { get; set; }
    }
}
