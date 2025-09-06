using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.BrandRequest
{
    public class CreateBrandRequest
    {
        [Required(ErrorMessage = "Mã thương hiệu không được để trống")]
        [MaxLength(20, ErrorMessage = "Mã thương hiệu không được vượt quá 20 ký tự")]
        [RegularExpression(@"^[A-Z0-9_-]+$", ErrorMessage = "Mã thương hiệu chỉ được chứa chữ hoa, số, dấu gạch ngang và gạch dưới")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên thương hiệu không được vượt quá 100 ký tự")]
        public string Name { get; set; }
    }
}
