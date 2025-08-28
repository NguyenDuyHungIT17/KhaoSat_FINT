using System.IO;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace KhaoSat.Utils
{
    public class MailHelper
    {
        private readonly IConfiguration _config;

        public MailHelper(IConfiguration config)
        {
            _config = config;
        }

        public void SendResetCode(string toEmail, string code)
        {
            string fromEmail = _config["MailSettings:From"];
            string username = _config["MailSettings:Username"];
            string password = _config["MailSettings:Password"];
            string host = _config["MailSettings:Host"];
            int port = int.Parse(_config["MailSettings:Port"]);

            // Đọc template từ file
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "ResetPassword.html");
            string body = File.ReadAllText(templatePath);
            body = body.Replace("{{CODE}}", code);

            Console.WriteLine($"From: {fromEmail}, To: {toEmail}");

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(fromEmail);
                mail.To.Add(toEmail);
                mail.Subject = "Mã xác nhận đặt lại mật khẩu";
                mail.Body = body;
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient(host, port))
                {
                    smtp.Credentials = new NetworkCredential(username, password);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }

    }
}
