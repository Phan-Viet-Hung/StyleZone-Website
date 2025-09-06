using System;

namespace API.Domain.DTOs
{
    public class BrandDto
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int QuantityProduct { get; set; }
        public int TotalQuantity { get; set; }
    }
}
