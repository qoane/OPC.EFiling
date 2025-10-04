using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Domain.Entities;
using OPC.EFiling.Infrastructure.Data;
using System.Threading.Tasks;

namespace OPC.EFiling.API.Controllers
{
    /// <summary>
    /// Provides endpoints to reassign instructions either from one PC to another
    /// or from one drafter to another.  The Registrar can reassign a PC when
    /// the original PC is unavailable.  A PC can reassign to a different drafter
    /// within the hierarchy.  These endpoints update the AssignedDrafterID
    /// and Status accordingly.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InstructionReassignmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InstructionReassignmentController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Registrar reassigns an instruction to a different PC.  The new
        /// PC’s user ID is stored in AssignedDrafterID and the status is
        /// reset to "PC Assigned".
        /// </summary>
        [HttpPost("ReassignPc")]
        [Authorize(Roles = "RegistryOfficer")]
        public async Task<IActionResult> ReassignToPc([FromBody] ReassignDto dto)
        {
            var instruction = await _context.DraftingInstructions.FindAsync(dto.InstructionId);
            if (instruction == null)
                return NotFound("Instruction not found.");

            instruction.AssignedDrafterID = dto.NewUserId;
            instruction.Status = "PC Assigned";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Instruction reassigned to PC successfully." });
        }

        /// <summary>
        /// PC reassigns an instruction to a different drafter.  The new
        /// drafter’s user ID is stored in AssignedDrafterID and the status
        /// is set to "Assigned".
        /// </summary>
        [HttpPost("ReassignDrafter")]
        [Authorize(Roles = "PC")]
        public async Task<IActionResult> ReassignToDrafter([FromBody] ReassignDto dto)
        {
            var instruction = await _context.DraftingInstructions.FindAsync(dto.InstructionId);
            if (instruction == null)
                return NotFound("Instruction not found.");

            instruction.AssignedDrafterID = dto.NewUserId;
            instruction.Status = "Assigned";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Instruction reassigned to drafter successfully." });
        }

        public class ReassignDto
        {
            public int InstructionId { get; set; }
            public int NewUserId { get; set; }
        }
    }
}