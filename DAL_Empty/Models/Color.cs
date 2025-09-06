using System.ComponentModel.DataAnnotations;

namespace DAL_Empty.Models
{
    public class Color
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(10, ErrorMessage = "Mã màu không được vượt quá 10 ký tự.")]
        public string? Code { get; set; }
        [Required(ErrorMessage = "Tên màu không được để trống.")]
        [MaxLength(50, ErrorMessage = "Tên màu không được vượt quá 50 ký tự.")]
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();
    }
}
