namespace API.Domain.DTOs
{
    public class ColorDto
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; } = string.Empty;
        public int QuantityProduct { get; set; }
        public int TotalQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

}
