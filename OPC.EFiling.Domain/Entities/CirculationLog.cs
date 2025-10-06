using System;
using System.Collections.Generic;

namespace OPC.EFiling.Domain.Entities
{
    public class CirculationLog
    {
        public int CirculationLogId { get; set; }

        public int DraftId { get; set; }
        public Draft Draft { get; set; } = null!;

        public string? VersionLabel { get; set; }
        public string SentToEmail { get; set; } = null!;
        public string? CcEmail { get; set; }
        public string Subject { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public int? SentByUserId { get; set; }
        public User? SentByUser { get; set; }
        public string? Notes { get; set; }

        public int? DocumentId { get; set; }
        public Document? Document { get; set; }

        /// <summary>
        /// Collection of responses received for this circulation log.
        /// Each response represents feedback from the recipient ministry.
        /// </summary>
        public ICollection<CirculationResponse> Responses { get; set; } = new List<CirculationResponse>();
    }
}
