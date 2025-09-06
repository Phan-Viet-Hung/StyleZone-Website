using API.DomainCusTomer.DTOs.CastCustomerId;

namespace API.DomainCusTomer.DTOs.ThanhToanCustomerId
{
    public class ThanhToanCartIdDto
    {
        public List<CastCustomerIDDto> CartItems { get; set; }
        public List<AddressDto> Addresses { get; set; }
        public List<VoucherDto> Vouchers { get; set; }
    }
}
