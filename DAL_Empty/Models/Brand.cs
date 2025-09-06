using System.ComponentModel.DataAnnotations;

namespace DAL_Empty.Models
{
    public class Brand
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Mã thương hiệu không được để trống")]
        [MaxLength(20, ErrorMessage = "Mã thương hiệu không được vượt quá 20 ký tự")]
        [Display(Name = "Mã thương hiệu")]
        [RegularExpression(@"^[A-Z0-9_-]+$", ErrorMessage = "Mã thương hiệu chỉ được chứa chữ hoa, số, dấu gạch ngang và gạch dưới")]
        public string? Code { get; set; }

        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên thương hiệu không được vượt quá 100 ký tự")]
        [Display(Name = "Tên thương hiệu")]
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Product> Product { get; set; } = new List<Product>();
    }
}
