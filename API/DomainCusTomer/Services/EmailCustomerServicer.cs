using API.DomainCusTomer.Config;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace API.DomainCusTomer.Services
{
    public class EmailCustomerServicer
    {
        private readonly MailSettings _mail;

        public EmailCustomerServicer(IOptions<MailSettings> options)
        {
            _mail = options.Value;
        }

        public async Task SendOtpAsync(string toEmail, string otp)
        {
            var msg = new MimeMessage();
            msg.From.Add(MailboxAddress.Parse(_mail.From));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = "Mã xác thực OTP";
            msg.Body = new TextPart("plain")
            {
                Text = $"Mã OTP của bạn là: {otp}. Có hiệu lực 2 phút."
            };
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_mail.Smtp, _mail.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mail.User, _mail.Pass);
            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }
    }
}
