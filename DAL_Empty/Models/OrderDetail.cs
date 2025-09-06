using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public class OrderDetail
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Số lượng không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int? Quantity { get; set; }
        [Required(ErrorMessage = "Giá không được để trống.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }
        [Required(ErrorMessage = "OrderId không được để trống.")]
        [ForeignKey("Order")]
        public Guid? OrderId { get; set; }
        [Required(ErrorMessage = "ProductDetailId không được để trống.")]
        [ForeignKey("ProductDetail")]
        public Guid? ProductDetailId { get; set; }

        public virtual OrderInfo? Order { get; set; }

        public virtual ProductDetail? ProductDetail { get; set; }

    }
}
