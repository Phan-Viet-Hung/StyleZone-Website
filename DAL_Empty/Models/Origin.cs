using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DAL_Empty.Models
{
    public class Origin
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Tên xuất xứ là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên xuất xứ không được vượt quá 100 ký tự")]
        public string? Name { get; set; }
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();
    }
}
