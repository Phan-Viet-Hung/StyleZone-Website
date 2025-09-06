using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL_Empty.Models
{
    public enum OrderStatus
    {
        Pending = 1,
        Confirmed = 2,
        Processing = 3,
        Shipping = 4,
        Delivered = 5,
        Cancelled = 6
    }


    public class OrderInfo
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Ngày tạo")]
        public DateTime? CreateAt { get; set; }
        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdateAt { get; set; }
        [Display(Name = "Người cập nhật")]
        public Guid? UpdateBy { get; set; }
        public Guid? CreateBy { get; set; }

        [Display(Name = "Khách hàng")]
        public Guid? CustomerId { get; set; }
        //[Required(ErrorMessage = "Tên người nhận không được để trống")]
        [MaxLength(100)]
        [Display(Name = "Tên người nhận")]
        public string? CustomerName { get; set; }
        //[Required(ErrorMessage = "Địa chỉ giao hàng không được để trống")]
        [MaxLength(200)]
        [Display(Name = "Địa chỉ giao hàng")]
        public string? Address { get; set; }
        //[Required(ErrorMessage = "Số điện thoại người nhận không được để trống")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại không đúng định dạng")]
        [MaxLength(15)]
        [Display(Name = "Số điện thoại người nhận")]
        public string? PhoneNumber { get; set; }


        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Phí vận chuyển")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? ShippingFee { get; set; }


        [Range(0.01, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng tiền")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? TotalAmount { get; set; }

        [MaxLength(500)]
        [Display(Name = "Ghi chú")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }
        [Required]
        [Display(Name = "Trạng thái")]
        public OrderStatus Status { get; set; }
        [Display(Name = "Mã QR")]
        public string? Qrcode { get; set; }
        [Display(Name = "Ngày giao hàng dự kiến")]
        [DataType(DataType.Date)]
        public DateTime? EstimatedDeliveryDate { get; set; }

        public virtual ICollection<OrderHistory> BillHistories { get; set; } = new List<OrderHistory>();
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<ModeOfPaymentOrder> ModeOfPaymentOrders { get; set; } = new List<ModeOfPaymentOrder>();

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public virtual ICollection<OrderPaymentMethod> OrderPaymentMethods { get; set; } = new List<OrderPaymentMethod>();

        public virtual Account? UpdateByNavigation { get; set; }
        public virtual Account? CreateByNavigation { get; set; }
    }
}
