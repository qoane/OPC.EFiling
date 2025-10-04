using System;

namespace OPC.EFiling.Domain.Entities
{
    public class CirculationLog
    {
        public int CirculationLogId { get; set; }

        // Link to the draft (latest or specific version you sent)
        public int DraftId { get; set; }
        public Draft? Draft { get; set; }

        // Optional: capture a version label/number if you want to tag it
        public string? VersionLabel { get; set; }   // e.g. "v1.0" or "v1.1"

        // Email details
        public string SentToEmail { get; set; } = string.Empty;
        public string? CcEmail { get; set; }
        public string Subject { get; set; } = string.Empty;

        // When and by whom (OPC user) this send occurred
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public int? SentByUserId { get; set; }
        public User? SentByUser { get; set; }

        // Optional notes: “First send to MDA”, “Addressed comments”, etc.
        public string? Notes { get; set; }

        // Optional: store the DocumentId if you persist the exported PDF
        public int? DocumentId { get; set; }
        public Document? Document { get; set; }
    }
}
