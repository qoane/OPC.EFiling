using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC.EFiling.Domain.Entities
{
    public class InstructionAttachment
    {
        public int InstructionAttachmentID { get; set; }

        public int DraftingInstructionID { get; set; }
        public DraftingInstruction DraftingInstruction { get; set; } = null!;

        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
