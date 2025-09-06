using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DAL_Empty.Models
{
    public class Image
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "URL hình ảnh không được để trống.")]
        [MaxLength(500, ErrorMessage = "URL hình ảnh không được vượt quá 500 ký tự.")]
        public string Url { get; set; }

        [Required(ErrorMessage = "Tên file không được để trống.")]
        [MaxLength(500, ErrorMessage = "Tên file không được vượt quá 100 ký tự.")]
        public string FileName { get; set; }

        [MaxLength(200, ErrorMessage = "Mô tả hình ảnh không được vượt quá 200 ký tự.")]
        public string? AltText { get; set; }

        public bool IsMainImage { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("ProductDetail")]
        public Guid? ProductDetailId { get; set; }
        public virtual ProductDetail? ProductDetail { get; set; }
    }

}
