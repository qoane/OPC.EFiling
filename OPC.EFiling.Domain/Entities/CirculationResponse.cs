using OPC.EFiling.Domain.Entities;

public class CirculationResponse
{
    public int CirculationResponseId { get; set; }

    // Correct FK definition
    public int CirculationLogId { get; set; }

    // Navigation property
    public CirculationLog CirculationLog { get; set; }

    public string? ResponseText { get; set; }
    public int? DocumentId { get; set; }
    public Document? Document { get; set; }

    public DateTime ReceivedAt { get; set; }
    public int? ReceivedByUserId { get; set; }
    public User? ReceivedByUser { get; set; }
}
