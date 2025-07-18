namespace OPC.EFiling.Domain.Entities
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public int ToUserID { get; set; }
        public string? Message { get; set; }
        public bool Seen { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}