using System;

namespace OPC.EFiling.Domain.Entities
{
    /// <summary>
    /// Represents a saved version of a draft document.  Each time a drafter
    /// saves or submits a draft, a new version is created with a sequential
    /// version number and associated metadata.
    /// </summary>
    public class DraftVersion
    {
        public int DraftVersionID { get; set; }

        /// <summary>
        /// Foreign key to the drafting instruction this version belongs to.
        /// </summary>
        public int DraftingInstructionID { get; set; }

        /// <summary>
        /// The HTML (or other format) content of the draft at this version.  This
        /// allows the system to restore or view the exact text that was saved
        /// at a given point in time.
        /// </summary>
        public string ContentHtml { get; set; } = string.Empty;

        /// <summary>
        /// A human‑readable version label (e.g., "1", "2", "3").
        /// </summary>
        public string VersionNumber { get; set; } = "1";

        /// <summary>
        /// Optional path to a saved file representing this version (for example,
        /// if you later export to PDF or Word).
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// The user who created this version.
        /// </summary>
        public int CreatedByUserID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional note describing the changes made in this version.
        /// </summary>
        public string? VersionNote { get; set; }
    }
}