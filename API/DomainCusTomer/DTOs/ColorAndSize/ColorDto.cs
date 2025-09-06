namespace API.DomainCusTomer.DTOs.Color
{
    public class ColorDto
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
