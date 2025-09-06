namespace API.Domain.DTOs
{
    public class ImageDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool IsMainImage { get; set; }
        public Guid? ProductDetailId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
