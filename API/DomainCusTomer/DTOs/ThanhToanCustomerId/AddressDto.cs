namespace API.DomainCusTomer.DTOs.ThanhToanCustomerId
{
    public class AddressDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string DetailAddress { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string Province { get; set; }
        public string Stastus { get; set; }
    }
}
