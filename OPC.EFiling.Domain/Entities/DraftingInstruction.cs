namespace OPC.EFiling.Domain.Entities
{
    public class DraftingInstruction
    {
        public int DraftingInstructionID { get; set; }

        public string? Title { get; set; }

        // ── NEW: “Instruction text” (body of what needs to be drafted)
        public string? Description { get; set; }

        public int DepartmentID { get; set; }
        public int AssignedDrafterID { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public DateTime ReceivedDate { get; set; }

        // ── Navigation for attachments
        public ICollection<InstructionAttachment> Files { get; set; } = new List<InstructionAttachment>();

        // ── Navigation for any drafts linked to this instruction
        public ICollection<Draft> Drafts { get; set; } = new List<Draft>();
    }

    

 
}
