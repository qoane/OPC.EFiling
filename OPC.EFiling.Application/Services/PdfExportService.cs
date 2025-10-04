using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OPC.EFiling.Domain.Entities;

// QuestPDF
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

// Alias to avoid clash with OPC.EFiling.Domain.Entities.Document
using QDocument = QuestPDF.Fluent.Document;

namespace OPC.EFiling.Application.Services
{
    public class PdfExportService : IPdfExportService
    {
        private static string HtmlToPlainText(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var text = html
                .Replace("</p>", "\n\n", StringComparison.OrdinalIgnoreCase)
                .Replace("<br>", "\n", StringComparison.OrdinalIgnoreCase)
                .Replace("<br/>", "\n", StringComparison.OrdinalIgnoreCase)
                .Replace("<br />", "\n", StringComparison.OrdinalIgnoreCase)
                .Replace("</li>", "\n", StringComparison.OrdinalIgnoreCase)
                .Replace("</div>", "\n", StringComparison.OrdinalIgnoreCase);

            text = Regex.Replace(text, "<.*?>", string.Empty);

            text = text.Replace("&nbsp;", " ")
                       .Replace("&amp;", "&")
                       .Replace("&lt;", "<")
                       .Replace("&gt;", ">")
                       .Replace("&quot;", "\"")
                       .Replace("&#39;", "'");

            text = Regex.Replace(text, "[ \t]+\n", "\n");
            text = Regex.Replace(text, "\n{3,}", "\n\n");
            return text.Trim();
        }

        public Task<byte[]> RenderDraftPdfAsync(Draft draft, string? versionLabel = null)
        {
            var instructionTitle = draft.DraftingInstruction?.Title?.Trim();
            var title = string.IsNullOrWhiteSpace(instructionTitle) ? "OPC Draft" : instructionTitle;
            var reference = draft.DraftingInstructionID?.ToString() ?? draft.DraftID.ToString();
            var bodyText = HtmlToPlainText(draft.ContentHtml);

            var brandBlue = "#0b2243";
            var subtle = Colors.Grey.Lighten3;

            byte[] bytes = QDocument.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2.0f, Unit.Centimetre);
                    page.DefaultTextStyle(s => s.FontSize(11));   // global default

                    // ===== HEADER =====
                    page.Header().Element(h =>
                    {
                        h.Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                // Title line
                                col.Item().Element(e =>
                                {
                                    e.DefaultTextStyle(s => s.Bold().FontSize(12).FontColor(brandBlue));
                                    e.Text("Office of Parliamentary Counsel");
                                });

                                // Sub line (version)
                                var headerLine = $"Draft {(string.IsNullOrWhiteSpace(versionLabel) ? "" : $"— {versionLabel}")}".Trim();
                                col.Item().Element(e =>
                                {
                                    e.DefaultTextStyle(s => s.FontSize(9).FontColor(Colors.Grey.Darken1));
                                    e.Text(headerLine);
                                });
                            });

                            // Right: Reference
                            row.ConstantItem(120).AlignRight().Element(e =>
                            {
                                e.DefaultTextStyle(s => s.FontSize(10));
                                e.Text(t =>
                                {
                                    t.Span("Ref: ").SemiBold();
                                    t.Span(reference);
                                });
                            });
                        });
                    });

                    // ===== CONTENT =====
                    page.Content().Element(c =>
                    {
                        c.Column(col =>
                        {
                            col.Spacing(8);

                            // Big heading
                            col.Item().Element(e =>
                            {
                                e.DefaultTextStyle(s => s.Bold().FontSize(18).FontColor(brandBlue));
                                e.Text(title);
                            });

                            // Meta line
                            var createdAt = draft.CreatedAt.ToLocalTime().ToString("dd MMM yyyy HH:mm");
                            var modified = draft.LastModifiedAt?.ToLocalTime().ToString("dd MMM yyyy HH:mm");

                            col.Item().Element(e =>
                            {
                                e.DefaultTextStyle(s => s.FontSize(9).FontColor(Colors.Grey.Darken1));
                                e.Text(t =>
                                {
                                    t.Span("Created: ").SemiBold();
                                    t.Span(createdAt);

                                    if (!string.IsNullOrWhiteSpace(modified))
                                    {
                                        t.Span("   ·   Last Modified: ").SemiBold();
                                        t.Span(modified!);
                                    }
                                });
                            });

                            col.Item().LineHorizontal(0.5f).LineColor(subtle);

                            // Body
                            if (string.IsNullOrWhiteSpace(bodyText))
                            {
                                col.Item().Element(e =>
                                {
                                    e.DefaultTextStyle(s => s.Italic().FontColor(Colors.Grey.Darken1));
                                    e.Text("(No draft content)");
                                });
                            }
                            else
                            {
                                foreach (var paragraph in bodyText.Split(new[] { "\n\n" }, StringSplitOptions.None))
                                {
                                    var p = paragraph.Trim();
                                    if (p.Length == 0) continue;

                                    col.Item().Element(e =>
                                    {
                                        e.DefaultTextStyle(s => s.FontSize(11));
                                        e.Text(p);
                                    });
                                }
                            }
                        });
                    });

                    // ===== FOOTER =====
                    page.Footer().AlignRight().Element(e =>
                    {
                        e.DefaultTextStyle(s => s.FontSize(9));
                        e.Text(t =>
                        {
                            t.Span("Generated by OPC E-Filing — ");
                            t.CurrentPageNumber();
                            t.Span("/");
                            t.TotalPages();

                            if (!string.IsNullOrWhiteSpace(versionLabel))
                                t.Span($" — {versionLabel}");
                        });
                    });
                });
            }).GeneratePdf();

            return Task.FromResult(bytes);
        }
    }
}
