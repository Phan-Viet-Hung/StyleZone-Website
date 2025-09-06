using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.OriginRequest
{
    public class UpdateOriginRequest : CreateOriginRequest
    {
        [Required(ErrorMessage = "ID là bắt buộc")]
        public Guid Id { get; set; }
    }
}
