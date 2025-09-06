using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.CategoryRequest
{
    public class UpdateCategoryRequest : CreateCategoryRequest
    {
        [Required(ErrorMessage = "Id là bắt buộc.")]
        public Guid Id { get; set; }
    }
}
