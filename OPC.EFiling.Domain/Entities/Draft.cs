using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC.EFiling.Domain.Entities
{
    public class Draft
    {
        public int DraftID { get; set; }

        public int CreatedByUserID { get; set; }

        public int? DraftingInstructionID { get; set; }
        public DraftingInstruction? DraftingInstruction { get; set; }

        public string ContentHtml { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }
    }
}
