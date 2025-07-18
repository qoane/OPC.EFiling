namespace OPC.EFiling.Domain.Entities
{
    public class AuditLog
    {
        public int AuditLogID { get; set; }
        public int UserID { get; set; }
        public string? ActionType { get; set; }
        public string? TargetEntity { get; set; }
        public DateTime Timestamp { get; set; }
    }
}