namespace OPC.EFiling.Domain.Entities
{
    public class Document
    {
        public int DocumentID { get; set; }
        public int InstructionID { get; set; }
        public string? Title { get; set; }
        public int Version { get; set; }
        public string? Status { get; set; }
        public int CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
    }
}