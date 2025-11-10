using System.Net.Mail;
using System.Net;

namespace ang_emp_api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_config["Smtp:Host"])
            {
                Port = int.Parse(_config["Smtp:Port"]),
                Credentials = new NetworkCredential(_config["Smtp:User"], _config["Smtp:Pass"]),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_config["Smtp:User"], "Asset Management System"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            await smtpClient.SendMailAsync(mail);
        }
        public void SendEmail(string to, string subject, string body)
        {
            Console.WriteLine($"[EMAIL] To: {to}, Subject: {subject}, Body: {body}");
        }
    }
}
/*
 using System.Net;
using System.Net.Mail;
using ang_emp_api.Models;
using ang_emp_api.DTOs;
using System.Text;

namespace ang_emp_api.Services
{
    public interface IEmailService
    {
        void SendEmail(string to, string subject, string body);
        void SendAssetAssignmentEmail(Employee employee, List<AssetDto> assignedAssets);
    }
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendEmail(string to, string subject, string body)
        {
            try
            {
                var fromAddress = new MailAddress(_config["EmailSettings:FromEmail"], "HR Department");
                var toAddress = new MailAddress(to);
                string fromPassword = _config["EmailSettings:AppPassword"];

                var smtp = new SmtpClient
                {
                    Host = _config["EmailSettings:SmtpServer"],
                    Port = int.Parse(_config["EmailSettings:Port"]),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }

        // ✅ Send formatted mail when assets are assigned
        public void SendAssetAssignmentEmail(Employee employee, List<AssetDto> assignedAssets)
        {
            var sb = new StringBuilder();
            sb.Append($"<h2>Asset Assignment Notification</h2>");
            sb.Append($"<p>Dear <strong>{employee.FullName}</strong>,</p>");
            sb.Append($"<p>The following IT assets have been assigned to you on {DateTime.Now:dd MMM yyyy}:</p>");
            sb.Append("<table style='border-collapse: collapse; width: 100%; border: 1px solid #ccc;'>");
            sb.Append("<tr style='background-color: #f2f2f2;'><th style='border:1px solid #ccc; padding:8px;'>Asset Name</th><th style='border:1px solid #ccc; padding:8px;'>Category</th><th style='border:1px solid #ccc; padding:8px;'>Serial No</th><th style='border:1px solid #ccc; padding:8px;'>Assigned Date</th></tr>");

            foreach (var asset in assignedAssets)
            {
                sb.Append("<tr>");
                sb.Append($"<td style='border:1px solid #ccc; padding:8px;'>{asset.AssetName}</td>");
                sb.Append($"<td style='border:1px solid #ccc; padding:8px;'>{asset.Category}</td>");
                sb.Append($"<td style='border:1px solid #ccc; padding:8px;'>{asset.SerialNumber}</td>");
                sb.Append($"<td style='border:1px solid #ccc; padding:8px;'>{asset.AssignedDate?.ToString("dd MMM yyyy")}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</table>");
            sb.Append("<br/><p>Please take care of the provided assets responsibly and report any issues to the IT department.</p>");
            sb.Append("<p>Regards,<br/><strong>IT Asset Management Team</strong></p>");

            SendEmail(employee.Email, "Assets Assigned - Company IT Department", sb.ToString());
        }
    }
}

 */
