using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public enum ReturnRequestStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Processing = 4,
        Completed = 5,
        Cancelled = 6
    }
    public class ReturnRequest
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Lý do trả hàng là bắt buộc")]
        [StringLength(500, ErrorMessage = "Lý do trả hàng không được vượt quá 500 ký tự")]
        public string? Reason { get; set; }
        [Required(ErrorMessage = "Ngày yêu cầu là bắt buộc")]
        [Column(TypeName = "datetime2")]
        public DateTime? RequestDate { get; set; }
        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public ReturnRequestStatus Status { get; set; }
        [Required(ErrorMessage = "ID chi tiết đơn hàng là bắt buộc")]
        public Guid? OrderDetailId { get; set; }
        [ForeignKey("OrderDetailId")]
        public virtual OrderDetail? OrderDetail { get; set; }
    }
}
