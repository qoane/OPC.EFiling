namespace OPC.EFiling.Domain.Entities
{
    public class ApprovalLog
    {
        public int ApprovalLogID { get; set; }
        public int DocumentID { get; set; }
        public int ApprovedBy { get; set; }
        public string? Status { get; set; }
        public string? Comment { get; set; }
        public DateTime DateActioned { get; set; }
    }
}