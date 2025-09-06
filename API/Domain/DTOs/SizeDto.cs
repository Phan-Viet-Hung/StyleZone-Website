namespace API.Domain.DTOs
{
    public class SizeDto
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
