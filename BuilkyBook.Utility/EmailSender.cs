using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuilkyBook.Utility
{
    public class EmailSender : IEmailSender
    {
        public string SendGridApi { get; set; }

        public EmailSender(IConfiguration configuration)
        {
            SendGridApi = configuration.GetValue<string>("SendGrid:APIKEY");
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SendGridClient(SendGridApi);
            var from = new EmailAddress("hasith450@gmail.com", "Builky Book");
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);

            return client.SendEmailAsync(msg);
        }
    }
}
