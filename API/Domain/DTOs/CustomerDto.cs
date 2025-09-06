using DAL_Empty.Models;

namespace API.Domain.DTOs
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string? Fullname { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserName { get; set; }
        public GenderEnum? Gender { get; set; }
        public string? Status { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }

    }
}
