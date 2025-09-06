using System.ComponentModel.DataAnnotations;
namespace API.Domain.Request.ColorRequest
{
    public class CreateColorRequest
    {
        [Required(ErrorMessage = "Mã màu không được để trống")]
        [MaxLength(10)]
        [RegularExpression("^#([A-Fa-f0-9]{6})$", ErrorMessage = "Mã màu phải theo định dạng hex, ví dụ: #FFFFFF")]
        public string? Code { get; set; }

        [Required(ErrorMessage = "Tên màu không được để trống")]
        [MaxLength(50)]
        [RegularExpression(@"^[^\d]*$", ErrorMessage = "Tên màu không được chứa số")]
        public string? Name { get; set; } = string.Empty;
    }
}

