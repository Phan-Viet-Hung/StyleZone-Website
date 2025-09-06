using API.Domain.Request.AccountRequest;

namespace API.Domain.Service.IService
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);

        // Trả về mã + resetToken (để test) — production: chỉ trả 200 OK
        Task<ForgotPasswordResponse> ForgotPasswordAsync(string emailOrUsername);

        // Reset bằng token + code
        Task<bool> ResetPasswordWithCodeAsync(string token, string code, string newPassword, string confirmPassword);
    }
}
