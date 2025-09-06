using API.DomainCusTomer.DTOs.ThanhToanCustomer;
using System.ComponentModel.DataAnnotations;

namespace API.DomainCusTomer.DTOs.ThanhToanCustomerId
{
    public class OrderCustomerIdDto
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 03, 05, 07, 08 hoặc 09 và có đúng 10 số.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
        public string Address { get; set; }

        [Range(0, 10000000, ErrorMessage = "Phí vận chuyển không hợp lệ.")]
        public decimal ShippingFee { get; set; }

        [Range(1, 1000000000, ErrorMessage = "Tổng tiền phải lớn hơn 0.")]
        public decimal TotalAmount { get; set; }

        // Chỉ cần string code ("cod" hoặc "momo"), mapping sang GUID trong service
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        public string PaymentMethodCode { get; set; }  // "cod" hoặc "momo"
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public bool? IsFromCart { get; set; }
        public Guid? VorcherId { get; set; }
        public string? GhiChu {  get; set; }

        [Required(ErrorMessage = "Đơn hàng không có sản phẩm.")]
        [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất 1 sản phẩm.")]
        public List<OrderItemDto> OrderItems { get; set; }
    }
}
