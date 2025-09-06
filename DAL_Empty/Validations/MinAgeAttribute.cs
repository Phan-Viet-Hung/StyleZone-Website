using System.ComponentModel.DataAnnotations;

namespace DAL_Empty.Validations
{
    public class MinAgeAttribute : ValidationAttribute
    {
        private readonly int _minAge;

        public MinAgeAttribute(int minAge)
        {
            _minAge = minAge;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            if (value is DateTime birthday)
            {
                var today = DateTime.Today;
                var age = today.Year - birthday.Year;
                if (birthday > today.AddYears(-age)) age--;

                if (age < _minAge)
                {
                    return new ValidationResult(ErrorMessage ?? $"Bạn phải đủ ít nhất {_minAge} tuổi.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
