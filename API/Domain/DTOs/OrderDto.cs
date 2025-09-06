using DAL_Empty.Models;

namespace API.Domain.DTOs
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public Guid? UpdateBy { get; set; }
        public Guid? CreateBy { get; set; }
        public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CreateByName { get; set; }
        public string? UpdateByName { get; set; }

        public decimal? ShippingFee { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Description { get; set; }
        public OrderStatus Status { get; set; }
        public string? Qrcode { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }

        public CustomerDto? Customer { get; set; }
        public AccountDto? UpdateByNavigation { get; set; }
        public AccountDto? CreateByNavigation { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; } = new();
        public List<ModeOfPaymentOrderDto> ModeOfPaymentOrders { get; set; } = new();
        public List<OrderPaymentMethodDto> OrderPaymentMethods { get; set; } = new();
        public List<OrderHistoryDto> BillHistories { get; set; } = new();
    }
}
