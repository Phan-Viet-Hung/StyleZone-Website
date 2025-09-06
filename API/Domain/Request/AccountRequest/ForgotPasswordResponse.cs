namespace API.Domain.Request.AccountRequest
{
    public class ForgotPasswordResponse
    {
        public string Code { get; set; }

        // Reset token (JWT) chứa email + code + exp (server dùng để verify)
        public string ResetToken { get; set; }

        public string Message { get; set; }
    }
}
