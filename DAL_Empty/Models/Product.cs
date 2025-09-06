using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả sản phẩm không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [Range(1, 3, ErrorMessage = "Giới tính không hợp lệ")]
        public GenderEnum Gender { get; set; }

        [Required(ErrorMessage = "Ngày tạo là bắt buộc")]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedAt { get; set; }

        [Required(ErrorMessage = "Người tạo là bắt buộc")]
        public Guid CreatedBy { get; set; }

        public Guid? UpdatedBy { get; set; }
       

        [Required]
        [ForeignKey("Category")]
        public Guid CategoryId { get; set; }

        [Required]
        [ForeignKey("Brand")]
        public Guid BrandId { get; set; }

        public virtual Category Category { get; set; } = null!;
        public virtual Brand Brand { get; set; } = null!;
        public virtual ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();
    }
}
