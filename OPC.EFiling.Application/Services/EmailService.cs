using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OPC.EFiling.Application.Services;
using OPC.EFiling.Domain.Entities;

using Microsoft.Extensions.Options;



namespace OPC.EFiling.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public EmailService(IOptions<SmtpSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(
    string to,
    string subject,
    string body,
    bool isHtml = false,
    string? cc = null,
    byte[]? attachmentBytes = null,
    string? attachmentName = null,
    string? attachmentMime = "application/pdf")
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.From, "OPC"),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(to);
            if (!string.IsNullOrEmpty(cc))
                mailMessage.CC.Add(cc);

            if (attachmentBytes != null && attachmentBytes.Length > 0)
            {
                var name = string.IsNullOrWhiteSpace(attachmentName) ? "draft.pdf" : attachmentName;
                var mime = string.IsNullOrWhiteSpace(attachmentMime) ? "application/pdf" : attachmentMime;
                mailMessage.Attachments.Add(new Attachment(new MemoryStream(attachmentBytes), name, mime));
            }

            await client.SendMailAsync(mailMessage);
        }


    }
}
