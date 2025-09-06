using System.ComponentModel.DataAnnotations;

namespace DAL_Empty.Models
{
    public class Size
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Mã size là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã size không được vượt quá 20 ký tự")]
        public string? Code { get; set; }
        [Required(ErrorMessage = "Tên size là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên size không được vượt quá 50 ký tự")]
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();
    }
}
