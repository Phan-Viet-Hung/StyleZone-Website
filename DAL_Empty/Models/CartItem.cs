using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public class CartItem
    {
        [Key]
        public Guid Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Lỗi số lượng!")]
        public int? Quantity { get; set; }
        [Column(TypeName = "decimal(18,3)")]
        public decimal? Price { get; set; }
        [Required(ErrorMessage = "CartId không được để trống.")]
        [ForeignKey("Cart")]
        public Guid? CartId { get; set; }
        [Required(ErrorMessage = "ProductDetailId không được để trống.")]
        [ForeignKey("ProductDetail")]
        public Guid? ProductDetailId { get; set; }

        public virtual Cart? Cart { get; set; }

        public virtual ProductDetail? ProductDetail { get; set; }
    }
}
