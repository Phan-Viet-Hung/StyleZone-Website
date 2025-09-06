using API.Domain.DTOs;
using DAL_Empty.Models;

namespace API.Domain.Extentions
{
    public static class PromotionExtensions
    {
        public static PromotionDto ToDto(this Promotion p) => new()
        {
            Id = p.Id,
            Name = p.Name!,
            DiscountType = p.DiscountType.ToString(),
            DiscountValue = p.DiscountValue ?? 0,
            StartDate = p.StartDate ?? DateTime.Now,
            EndDate = p.EndDate ?? DateTime.Now,
            Description = p.Description,
            Status = p.Status.ToString(),
            ImageUrl = p.ImageUrl
        };

    }
}
