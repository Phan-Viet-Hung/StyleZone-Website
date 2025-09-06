using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.MaterialRequest
{
    public class CreateMaterialRequest
    {
        [Required(ErrorMessage = "Tên chất liệu là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Tên chất liệu không được vượt quá 100 ký tự.")]
        public string Name { get; set; }

        [MaxLength(500, ErrorMessage = "Mô tả chất liệu không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }
    }
}
