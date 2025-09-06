using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public enum DiscountType
    {
        Percentage = 1,
        FixedAmount = 2
    }
    public enum VoucherStatus
    {
        Active = 1,
        Inactive = 2,
        Expired = 3
    }
    public class Voucher
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Mã voucher là bắt buộc")]
        [StringLength(50, ErrorMessage = "Mã voucher không được vượt quá 50 ký tự")]
        public string Code { get; set; }
        [Required(ErrorMessage = "URL hình ảnh không được để trống.")]
        [MaxLength(500, ErrorMessage = "URL hình ảnh không được vượt quá 500 ký tự.")]
        public string ImageUrl { get; set; }
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Loại giảm giá là bắt buộc")]
        [Range(1, 2, ErrorMessage = "Loại giảm giá không hợp lệ")]
        public DiscountType DiscountType { get; set; }

        [Required(ErrorMessage = "Giá trị giảm giá là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá trị giảm giá phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền đơn hàng tối thiểu không được âm")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinOrderAmount { get; set; } = 1000;

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        [Column(TypeName = "datetime2")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        [Column(TypeName = "datetime2")]
        public DateTime EndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lần sử dụng tối đa mỗi khách hàng phải lớn hơn hoặc bằng 0")]
        public int MaxUsagePerCustomer { get; set; } = 0;

        [Range(1, int.MaxValue, ErrorMessage = "Tổng số lần sử dụng phải lớn hơn 0")]
        public int TotalUsageLimit { get; set; } = 1;

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [Range(1, 3, ErrorMessage = "Trạng thái không hợp lệ")]
        public VoucherStatus Status { get; set; } = VoucherStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; } = new List<CustomerVoucher>();
    }
}
