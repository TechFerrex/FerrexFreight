using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;


namespace FerrexWeb.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _pass;
        private readonly string _from;
        private readonly string _fromName;

        public SmtpEmailSender(IConfiguration config)
        {
            _host = config["SmtpSettings:Host"];
            _port = int.Parse(config["SmtpSettings:Port"]);
            _user = config["SmtpSettings:User"];
            _pass = config["SmtpSettings:Pass"];
            _from = config["SmtpSettings:FromEmail"];
            _fromName = config["SmtpSettings:FromName"];
        }


        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            using var mail = new MailMessage
            {
                From = new MailAddress(_from, _fromName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mail.To.Add(to);

            using var smtp = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_user, _pass),
                EnableSsl = true
            };
            await smtp.SendMailAsync(mail);
        }
    }
}
