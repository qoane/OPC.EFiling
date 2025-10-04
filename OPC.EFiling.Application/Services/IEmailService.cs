using System.Threading.Tasks;

namespace OPC.EFiling.Application.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            string to,
            string subject,
            string body,
            bool isHtml = false,
            string? cc = null,
            byte[]? attachmentBytes = null,
            string? attachmentName = null,
            string? attachmentMime = "application/pdf");
    }
}
