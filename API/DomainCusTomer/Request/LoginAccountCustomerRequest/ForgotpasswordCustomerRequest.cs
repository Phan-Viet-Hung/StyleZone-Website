using System.ComponentModel.DataAnnotations;

namespace API.DomainCusTomer.Request.AccountCustomerRequest
{
    public class ForgotpasswordCustomerRequest
    {
        
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?""':;{}|<>]).{6,100}$",
         ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;
        public string Otp {  get; set; } 
    }
}
