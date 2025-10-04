using System;
namespace OPC.EFiling.Domain.Entities
{
    /// <summary>
    /// Represents a response received from a ministry after a draft has been circulated.
    /// Responses are logged manually by registry staff because ministries do not
    /// have direct access to the system.  A response may include comments and/or a
    /// reference to an uploaded document (e.g., the annotated PDF returned via email).
    /// </summary>
    public class CirculationResponse
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int CirculationResponseId { get; set; }

        /// <summary>
        /// Foreign key to the CirculationLog that this response relates to.
        /// </summary>
        public int CirculationLogId { get; set; }

        /// <summary>
        /// Navigation property to the associated CirculationLog.
        /// </summary>
        public CirculationLog? CirculationLog { get; set; }

        /// <summary>
        /// The textual response/comments provided by the ministry.
        /// </summary>
        public string? ResponseText { get; set; }

        /// <summary>
        /// Optional link to a Document if the ministry returned an updated PDF or other
        /// attachment that was stored in the system.
        /// </summary>
        public int? DocumentId { get; set; }

        /// <summary>
        /// Navigation property to the Document associated with this response.
        /// </summary>
        public Document? Document { get; set; }

        /// <summary>
        /// When the response was logged in the system.
        /// </summary>
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The OPC user who recorded this response.
        /// </summary>
        public int? ReceivedByUserId { get; set; }

        /// <summary>
        /// Navigation property to the user who recorded the response.
        /// </summary>
        public User? ReceivedByUser { get; set; }
    }
}