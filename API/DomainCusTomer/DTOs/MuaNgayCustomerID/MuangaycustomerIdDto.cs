using API.DomainCusTomer.DTOs.ThanhToanCustomerId;

namespace API.DomainCusTomer.DTOs.MuaNgayCustomerID
{
    public class MuangaycustomerIdDto
    {
        public Guid Id { get; set; }
        public Guid ProductDetailId { get; set; }
        public string ProductDetailcode { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public string? ImageUrl { get; set; }
        public string? ColorName { get; set; }
        public string? ColorCode { get; set; }
        public string? SizeName { get; set; }
        public List<AddressDto> AddressList { get; set; } = new List<AddressDto>();
        public List<VoucherDto> VoucherList { get; set; } = new List<VoucherDto>();
    }
}
