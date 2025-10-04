using System.Threading.Tasks;
using OPC.EFiling.Domain.Entities;

namespace OPC.EFiling.Application.Services
{
    public interface IPdfExportService
    {
        /// <summary>
        /// Renders a branded PDF for a given Draft.
        /// </summary>
        /// <param name="draft">Draft containing (at minimum) Title, HtmlContent/Text, and reference info.</param>
        /// <param name="versionLabel">Optional version tag to print on the PDF header/footer.</param>
        /// <returns>PDF file as bytes.</returns>
        Task<byte[]> RenderDraftPdfAsync(Draft draft, string? versionLabel = null);
    }
}