using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.MaterialRequest
{
    public class UpdateMaterialRequest : CreateMaterialRequest
    {
        [Required(ErrorMessage = "Id là bắt buộc.")]
        public Guid Id { get; set; }
    }
}
