using DAL_Empty.Models;

namespace API.Domain.DTOs
{
    public class ModeOfPaymentDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Creator { get; set; }
        public string? Fixer { get; set; }
        public PaymentStatusEnum Status { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? EditDate { get; set; }
    }
}
