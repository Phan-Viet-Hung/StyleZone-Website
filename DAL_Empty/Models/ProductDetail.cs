using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public enum ProductDetailStatus
    {
        Active = 1,
        Inactive = 2,
        OutOfStock = 3
    }
    public class ProductDetail
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc")]
        [StringLength(200, ErrorMessage = "Mã sản phẩm không vượt quá 200 ký tự")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Tên chi tiết sản phẩm là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên chi tiết sản phẩm không được vượt quá 200 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public int? Quantity { get; set; }


        [Required(ErrorMessage = "Product ID là bắt buộc")]
        [ForeignKey("Product")]
        public Guid ProductId { get; set; }

        [ForeignKey("Color")]
        public Guid? ColorId { get; set; }

        [ForeignKey("Size")]
        public Guid? SizeId { get; set; }

        [ForeignKey("Material")]
        public Guid? MaterialId { get; set; }

        [ForeignKey("Origin")]
        public Guid? OriginId { get; set; }

        [ForeignKey("Supplier")]
        public Guid? SupplierId { get; set; }
        [Column(TypeName = "int")]
        public ProductDetailStatus Status { get; set; } = ProductDetailStatus.Active;

        // Navigation properties
      
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual Color? Color { get; set; } = null!;
        public virtual ICollection<Image> Images { get; set; } = new List<Image>();
        public virtual Material? Material { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual Origin? Origin { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual ICollection<PromotionProduct> PromotionProducts { get; set; } = new List<PromotionProduct>();
        //public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public virtual Size? Size { get; set; } = null!;
        public virtual Supplier? Supplier { get; set; } = null!;
    }
}
