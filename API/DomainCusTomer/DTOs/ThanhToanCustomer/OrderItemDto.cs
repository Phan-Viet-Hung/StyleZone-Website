using System.ComponentModel.DataAnnotations;

namespace API.DomainCusTomer.DTOs.ThanhToanCustomer
{
    public class OrderItemDto
    {
        [Required(ErrorMessage = "Thiếu ProductDetailId.")]
        public Guid ProductDetailId { get; set; }

        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100.")]
        public int Quantity { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Giá sản phẩm phải lớn hơn 0.")]
        public decimal Price { get; set; }
    }
}
