using System.ComponentModel.DataAnnotations;

namespace API.Domain.Validate
{
    public static class ValidationHelper
    {
        public static List<string> ValidateObject(object obj)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(obj, serviceProvider: null, items: null);
            bool valid = Validator.TryValidateObject(obj, context, results, true);

            return results.Select(r => r.ErrorMessage ?? "").ToList();
        }
    }

}
