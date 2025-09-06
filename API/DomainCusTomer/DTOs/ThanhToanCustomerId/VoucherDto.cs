using DAL_Empty.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.DomainCusTomer.DTOs.ThanhToanCustomerId
{
    public class VoucherDto
    {
       
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string? Description { get; set; }
        public decimal DiscountValue { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DiscountType DiscountType { get; set; }

        // Số tiền đơn hàng tối thiểu để áp dụng (có thể để trống)
        public decimal? MinOrderAmount { get; set; }

        // Ngày bắt đầu áp dụng
        public DateTime StartDate { get; set; }

        // Ngày kết thúc áp dụng
        public DateTime EndDate { get; set; }
        public int? TotalUsageLimit { get; set; }

        // Trạng thái voucher (mặc định là Active)
        public VoucherStatus Status { get; set; }
        public int? MaxUsagePerCustomer { get; set; }
    }
}
