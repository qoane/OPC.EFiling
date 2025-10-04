namespace OPC.EFiling.Application.DTOs
{
    /// <summary>
    /// DTO used when registry staff manually record a response from a ministry.
    /// The response text captures the content of the feedback email, and
    /// DocumentId can be provided if an annotated PDF has been uploaded separately.
    /// </summary>
    public class RecordResponseDto
    {
        /// <summary>
        /// Free-form comments from the ministry.  May be null if feedback is only
        /// provided via an attached document.
        /// </summary>
        public string? ResponseText { get; set; }

        /// <summary>
        /// Optional identifier of a Document that stores the ministry's returned PDF.
        /// </summary>
        public int? DocumentId { get; set; }
    }
}