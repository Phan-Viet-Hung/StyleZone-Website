namespace API.Configuration
{
    public class SmtpSettings
    {
        public string From { get; set; }
        public string Smtp { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }
    }
}