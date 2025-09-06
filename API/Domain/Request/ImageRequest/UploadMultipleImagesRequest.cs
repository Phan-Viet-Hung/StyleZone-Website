using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.ImageRequest
{
    public class UploadMultipleImagesRequest
    {
        [Required]
        public Guid ProductDetailId { get; set; }

        public List<IFormFile>? Files { get; set; } = new();

        public List<string>? Urls { get; set; } = new();

        public int? MainImageIndex { get; set; } // chỉ có 1 ảnh chính, tính theo tổng Files + Urls

        public List<string?>? AltTexts { get; set; } = new();
    }
}
