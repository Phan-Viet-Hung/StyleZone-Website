namespace API.Domain.DTOs
{
    public class OrderHistoryDto
    {
        public Guid Id { get; set; }
        public Guid? BillId { get; set; }
        public string? Description { get; set; }
        public string? Amount { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
