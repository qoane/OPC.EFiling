using System;

namespace OPC.EFiling.Domain.Entities
{
    /// <summary>
    /// Represents a captured signature for a drafting instruction. Each signature
    /// stores the signer's name, a base64 encoded PNG of the drawn signature and
    /// the timestamp of signing. Signatures are associated with a drafting
    /// instruction rather than a specific draft version so that final approval
    /// signatures persist across edits.
    /// </summary>
    public class Signature
    {
        public int SignatureId { get; set; }

        /// <summary>
        /// Foreign key to the drafting instruction being signed.
        /// </summary>
        public int DraftingInstructionId { get; set; }

        /// <summary>
        /// The display name of the user who signed. Captured at the time of
        /// signing for audit purposes.
        /// </summary>
        public string SignerName { get; set; } = string.Empty;

        /// <summary>
        /// Base64 encoded PNG image of the signature captured from a canvas on
        /// the front‑end. This string is stored as text in the database.
        /// </summary>
        public string ImageData { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the signature was created.
        /// </summary>
        public DateTime SignedAt { get; set; }
    }
}