using API.DomainCusTomer.DTOs.MuaNgayCustomerID;
using DAL_Empty.Models;

namespace API.DomainCusTomer.ExTentions
{
    public static class MuangayCustomerIDExtensions
    {
        public static MuangaycustomerIdDto ToMuaNgayIdDto(this ProductDetail p)
        {
            return new MuangaycustomerIdDto
            {
                ProductDetailId = p.Id,
                ColorName = p.Color.Name,
                SizeName = p.Size.Name,
                ProductDetailcode = p.Code,
                Name = p.Name,
                Price = p.Price,
                ImageUrl = p.Images.FirstOrDefault().Url

            };
        }
       
    }
}
