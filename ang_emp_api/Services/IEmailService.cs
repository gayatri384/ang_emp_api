using System;
namespace ang_emp_api.Services
{
    public interface IEmailService
    {
        // void SendEmail(string to, string subject, string body);
        void SendEmail(string to, string subject, string body);  // for console test
        Task SendEmailAsync(string to, string subject, string body);  // for real email
    }
}
