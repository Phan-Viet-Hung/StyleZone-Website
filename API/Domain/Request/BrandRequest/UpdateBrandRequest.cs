using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.BrandRequest
{
    public class UpdateBrandRequest : CreateBrandRequest
    {
        [Required]
        public Guid Id { get; set; }
    }
}
