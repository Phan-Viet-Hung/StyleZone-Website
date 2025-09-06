using API.Domain.Validate.IExcelValidator;
using DAL_Empty.Models;

namespace API.Domain.Validate
{
    public class ProductDetailValidator : IExcelValidator<ProductDetail>
    {
        public List<string> Validate(ProductDetail entity)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(entity.Name))
                errors.Add("Tên sản phẩm không được để trống");

            if (entity.Price <= 0)
                errors.Add("Giá phải lớn hơn 0");

            return errors;
        }
    }

}
