using System.ComponentModel.DataAnnotations;

public class UpdateImageRequest
{
    [Required]
    public Guid Id { get; set; }

    [Url(ErrorMessage = "URL hình ảnh không hợp lệ.")]
    [MaxLength(500)]
    public string? Url { get; set; }
    public IFormFile? File { get; set; }

    [MaxLength(100)]
    public string? FileName { get; set; }

    [MaxLength(200)]
    public string? AltText { get; set; }

    public bool? IsMainImage { get; set; } // nullable => chỉ update nếu được cung cấp
}
