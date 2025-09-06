namespace API.Domain.DTOs
{
    public class ImportResult
    {
        public int SuccessCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<AccountWithPasswordDto> CreatedAccounts { get; set; } = new();
    }
}
