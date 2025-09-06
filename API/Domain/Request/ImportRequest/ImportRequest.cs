using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.ImportRequest
{
    public class ImportRequest
    {
        [Required]
        public string EntityName { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }
}
