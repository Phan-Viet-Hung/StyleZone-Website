namespace API.DomainCusTomer.DTOs.Tintuc
{
    public class TinTucDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? ImageUrl { get; set; }
        public string? Content { get; set; } 
        public DateTime CreatedDate { get; set; }
    }
}
