namespace API.Domain.DTOs
{
    public class AccountWithPasswordDto:AccountDto
    {
        public string RawPassword { get; set; }
    }
}
