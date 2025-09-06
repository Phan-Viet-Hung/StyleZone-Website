using Microsoft.Extensions.Caching.Memory;

namespace API.DomainCusTomer.Services
{
    public class OtpHelperServices
    {
        private readonly IMemoryCache _cache;
        public OtpHelperServices(IMemoryCache cache) => _cache = cache;

        public string Generate(string email)
        {
            string code = Random.Shared.Next(100000, 999999).ToString();
            _cache.Set($"OTP:{email}", code, TimeSpan.FromMinutes(2));
            return code;
        }

        public bool Verify(string email, string otp) =>
           _cache.TryGetValue($"OTP:{email}", out string? saved) && saved == otp;
    }
}
