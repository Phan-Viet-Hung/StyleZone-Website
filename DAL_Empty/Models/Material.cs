using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DAL_Empty.Models
{
    public class Material
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Tên chất liệu không được để trống.")]
        [MaxLength(100, ErrorMessage = "Tên chất liệu không được vượt quá 100 ký tự.")]
        public string? Name { get; set; }
        [MaxLength(500, ErrorMessage = "Mô tả chất liệu không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        // Timestamp cho audit
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();
    }
}
