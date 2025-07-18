using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Domain.Entities;
using OPC.EFiling.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OPC.EFiling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PCAllocationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PCAllocationController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/PCAllocation/LoggedInstructions
        /// Returns instructions marked as "Logged" (awaiting PC assignment).
        /// </summary>
        [HttpGet("LoggedInstructions")]
        [Authorize]
        public IActionResult GetLoggedInstructions()
        {
            var list = _context.DraftingInstructions
                .Where(i => i.Status == "Logged")
                .Select(i => new
                {
                    i.DraftingInstructionID,
                    i.Title,
                    i.Description,
                    i.DepartmentID,
                    i.Priority,
                    i.ReceivedDate
                })
                .ToList();

            return Ok(list);
        }

        /// <summary>
        /// PUT /api/PCAllocation/AssignDrafter/{id}
        /// Assigns a drafter and updates status to "Assigned".
        /// </summary>
        [HttpPut("AssignDrafter/{id}")]
        [Authorize]
        public async Task<IActionResult> AssignDrafter(int id, [FromBody] AssignDrafterDto dto)
        {
            var instruction = await _context.DraftingInstructions.FindAsync(id);
            if (instruction == null)
                return NotFound("Instruction not found.");

            instruction.AssignedDrafterID = dto.DrafterID;
            instruction.Status = "Assigned";
            instruction.Priority = dto.Priority;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Instruction assigned successfully." });
        }

        public class AssignDrafterDto
        {
            public int DrafterID { get; set; }
            public string Priority { get; set; } = "Medium";
        }
    }
}
