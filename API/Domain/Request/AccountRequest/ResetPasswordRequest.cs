namespace API.Domain.Request.AccountRequest
{
    public class ResetPasswordRequest
    {
        // token nhận được từ /forgot-password response (hoặc email link)
        public string Token { get; set; }

        // mã 6 chữ số mà user nhận trong email
        public string Code { get; set; }

        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
