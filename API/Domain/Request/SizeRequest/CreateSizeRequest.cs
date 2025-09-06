using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.SizeRequest
{
    public class CreateSizeRequest
    {
        [MaxLength(10)]
        public string? Code { get; set; }

        [MaxLength(50)]
        public string? Name { get; set; } = string.Empty;
    }
}
