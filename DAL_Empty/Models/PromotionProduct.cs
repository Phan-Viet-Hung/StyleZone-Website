using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;

namespace DAL_Empty.Models
{
    public class PromotionProduct
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Promotion ID là bắt buộc")]
        [ForeignKey("Promotion")]
        public Guid PromotionId { get; set; }

        [Required(ErrorMessage = "Product Detail ID là bắt buộc")]
        [ForeignKey("ProductDetail")]
        public Guid ProductDetailId { get; set; }
        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Pricebeforereduction { get; set; }
        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Priceafterduction { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public virtual ProductDetail? ProductDetail { get; set; }

        public virtual Promotion? Promotion { get; set; }
    }
}
