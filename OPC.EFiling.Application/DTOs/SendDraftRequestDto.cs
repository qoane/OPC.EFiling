namespace OPC.EFiling.Application.DTOs
{
    public class SendDraftRequestDto
    {
        public string ToEmail { get; set; } = string.Empty;
        public string? CcEmail { get; set; }
        public string? Subject { get; set; }      // optional; will fall back to sensible default
        public string? MessageHtml { get; set; }  // optional; safe HTML
        public string? VersionLabel { get; set; } // optional; e.g. "v1.0"
        public string? Notes { get; set; }        // optional circulation notes for audit
    }
}