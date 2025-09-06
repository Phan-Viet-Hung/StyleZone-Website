namespace API.DomainCusTomer.DTOs.Tintuc
{
    public class TinTucDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? ShortDescription { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? ImageUrl { get; set; }
    }
}
