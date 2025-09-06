using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public enum RatingStatus
    {
        Active = 1,
        Inactive = 2,
        Pending = 3,
        Approved = 4,
        Rejected = 5
    }
    public class Rating
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public RatingStatus Status { get; set; }
        [Required(ErrorMessage = "ID người đánh giá là bắt buộc")]
        public string? RatingBy { get; set; }
        [Required(ErrorMessage = "Điểm đánh giá là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5")]
        public int? RatingScore { get; set; }
        [StringLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự")]
        public string? Comment { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime? CreateAt { get; set; }

        //[Column(TypeName = "datetime2")]
        //public DateTime? UpdateAt { get; set; }
        //[Required(ErrorMessage = "ID chi tiết sản phẩm là bắt buộc")]
        //public Guid? ProductDetailId { get; set; }
        //[ForeignKey("ProductDetailId")]
        //public virtual ProductDetail? ProductDetail { get; set; }
    }
}
