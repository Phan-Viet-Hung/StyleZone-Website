using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.PromotionRequest
{
    public class UpdatePromotionRequest : CreatePromotionRequest
    {
        [Required(ErrorMessage = "ID là bắt buộc")]
        public Guid Id { get; set; }
    }
}
