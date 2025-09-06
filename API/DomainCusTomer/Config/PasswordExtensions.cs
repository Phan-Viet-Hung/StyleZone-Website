using System.Security.Cryptography;
using System.Text;


namespace API.DomainCusTomer.Config
{
    public static class PasswordExtensions
    {
        // Ma Hoa luu vao database 
        public static string HashPassword(this string plainText)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hashBytes);
        }
        // So sanh ma hoa
        public static bool VerifyPassword(this string plainText, string hashedPassword)
        {
            return plainText.HashPassword().Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
        // An thong tin 

        public static string Mask(this string? value)
            => value == null ? string.Empty : new string('*', value.Length);

        public static string MaskEmail(this string email)
        {
            if (string.IsNullOrEmpty(email)) return string.Empty;

            var parts = email.Split('@');
            if (parts.Length != 2) return new string('*', email.Length);

            var namePart = parts[0];
            var domainPart = parts[1];

            var maskedName = namePart.Length <= 1
                ? "*"
                : namePart.Substring(0, 1) + new string('*', namePart.Length - 1);

            return $"{maskedName}@{domainPart}";
        }


    }
}
