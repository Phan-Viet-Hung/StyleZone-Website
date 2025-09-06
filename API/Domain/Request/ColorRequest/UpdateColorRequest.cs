using System.ComponentModel.DataAnnotations;
namespace API.Domain.Request.ColorRequest
{
    public class UpdateColorRequest : CreateColorRequest
    {
        [Required]
        public Guid Id { get; set; }
    }
}

