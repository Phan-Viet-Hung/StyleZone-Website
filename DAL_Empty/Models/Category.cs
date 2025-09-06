using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DAL_Empty.Models
{
    public class Category
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Tên danh mục không được để trống.")]
        [MaxLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự.")]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
