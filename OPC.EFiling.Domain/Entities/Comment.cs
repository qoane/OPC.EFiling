using System;
using System.Collections.Generic;

namespace OPC.EFiling.Domain.Entities
{
    /// <summary>
    /// Represents a threaded comment attached to a drafting instruction and optionally
    /// anchored to a particular selection of text within a draft. Comments may
    /// themselves have replies, allowing reviewers to hold a conversation around
    /// specific portions of a document. A comment may be resolved when the issue
    /// raised has been addressed.
    /// </summary>
    public class Comment
    {
        public int CommentId { get; set; }

        /// <summary>
        /// Foreign key to the DraftingInstruction this comment belongs to. Comments
        /// are anchored to the instruction rather than a specific draft version so
        /// they persist across saves and submissions. When displaying comments in
        /// the editor the front‑end may highlight the selection where the comment
        /// was originally created.
        /// </summary>
        public int DraftingInstructionID { get; set; }

        /// <summary>
        /// The user‑friendly name of the author who left the comment. This is
        /// captured at the time the comment is created and not updated if the
        /// user later changes their profile.
        /// </summary>
        public string AuthorName { get; set; } = string.Empty;

        /// <summary>
        /// The comment text. May include line breaks and markdown. HTML is not
        /// allowed and should be escaped on input to prevent XSS.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// When true the comment is considered resolved and should be displayed
        /// collapsed in the UI. Comments remain in the database for audit
        /// purposes even after resolution.
        /// </summary>
        public bool IsResolved { get; set; } = false;

        /// <summary>
        /// The time when the comment was created. Stored in UTC.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Optional parent comment ID if this comment is a reply to another
        /// comment. Replies inherit the DraftingInstructionID of the root
        /// comment.
        /// </summary>
        public int? ParentCommentId { get; set; }

        /// <summary>
        /// Navigation property to the parent comment.
        /// </summary>
        public Comment? ParentComment { get; set; }

        /// <summary>
        /// Replies to this comment. Replies form a flat tree structure.
        /// </summary>
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();

        /// <summary>
        /// A serialized representation of the selection range in the document
        /// where the comment was created. The client can use this to highlight
        /// the relevant portion in the editor. The format is arbitrary; for
        /// example, it could be a JSON pointer or a start/end index. Empty if
        /// the comment applies to the entire document.
        /// </summary>
        public string? Selection { get; set; }
    }
}