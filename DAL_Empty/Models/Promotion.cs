using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public class Promotion
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên khuyến mãi là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên khuyến mãi không được vượt quá 200 ký tự")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Loại giảm giá là bắt buộc")]
        public DiscountType DiscountType { get; set; }

        [Required(ErrorMessage = "Giá trị giảm giá là bắt buộc")]
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountValue { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        [Column(TypeName = "datetime2")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        [Column(TypeName = "datetime2")]
        public DateTime? EndDate { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public VoucherStatus Status { get; set; }

        [StringLength(255)]
        [Column("URL")]
        public string? ImageUrl { get; set; }

        public virtual ICollection<PromotionProduct> PromotionProducts { get; set; } = new List<PromotionProduct>();

    }
}
