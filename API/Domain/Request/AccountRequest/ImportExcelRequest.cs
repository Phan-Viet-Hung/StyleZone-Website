using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.AccountRequest
{
    public class ImportExcelRequest
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
