using API.Domain.Request.PromotionRequest;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVC.Areas.ViewModel
{
    public class CreatePromotionViewModel
    {
        public CreatePromotionRequest Request { get; set; } = new();

        public List<SelectListItem> ProductOptions { get; set; } = new();
        public List<SelectListItem> DiscountTypeOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();

    }
}
