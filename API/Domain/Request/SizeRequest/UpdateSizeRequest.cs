using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.SizeRequest
{
    public class UpdateSizeRequest : CreateSizeRequest
    {
        [Required]
        public Guid Id { get; set; }
    }
}
