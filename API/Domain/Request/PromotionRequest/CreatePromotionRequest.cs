using API.Domain.Validate;
using DAL_Empty.Models;
using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.PromotionRequest
{
    public class CreatePromotionRequest
    {
        [Required(ErrorMessage = "Tên chương trình không được để trống.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên chương trình từ 3 đến 100 ký tự.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Loại giảm giá không được để trống.")]
        [EnumDataType(typeof(DiscountType), ErrorMessage = "Loại giảm giá không hợp lệ.")]
        public DiscountType DiscountType { get; set; }

        [Required(ErrorMessage = "Giá trị giảm không được để trống.")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        [Range(0.01, 100000000, ErrorMessage = "Giá trị giảm phải lớn hơn 0.")]
        public decimal DiscountValue { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        [EnumDataType(typeof(VoucherStatus), ErrorMessage = "Trạng thái không hợp lệ.")]
        public VoucherStatus Status { get; set; } = VoucherStatus.Active;

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống.")]
        [DateGreaterThan("StartDate", ErrorMessage = "Ngày kết thúc phải lớn hơn ngày bắt đầu.")]
        public DateTime EndDate { get; set; }

        public string? ImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        public string? Description { get; set; }
        public List<Guid> ProductDetailIds { get; set; } = new();


    }
}
