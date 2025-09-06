using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.OriginRequest
{
    public class CreateOriginRequest
    {
        [Required(ErrorMessage = "Tên xuất xứ là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên xuất xứ không được vượt quá 100 ký tự")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }
    }
}
