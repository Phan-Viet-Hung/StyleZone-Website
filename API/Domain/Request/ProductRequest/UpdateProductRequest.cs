using DAL_Empty.Models;
using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.ProductRequest
{
    public class UpdateProductRequest
    {
        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc")]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự")]
        public string Name { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [Range(0, 2, ErrorMessage = "Giới tính không hợp lệ")]
        public GenderEnum Gender { get; set; }

        [Required(ErrorMessage = "Category là bắt buộc")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Brand là bắt buộc")]
        public Guid BrandId { get; set; }

        [Required(ErrorMessage = "Người cập nhật là bắt buộc")]
        public Guid UpdatedBy { get; set; }
    }
}
