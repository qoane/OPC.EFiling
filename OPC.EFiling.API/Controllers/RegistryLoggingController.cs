using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Domain.Entities;
using OPC.EFiling.Infrastructure.Data;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OPC.EFiling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistryLoggingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RegistryLoggingController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/RegistryLogging/Unlogged
        /// Returns instructions with Status = "Submitted" (pending registry logging).
        /// </summary>
        [HttpGet("Unlogged")]
        [Authorize]
        public IActionResult GetUnloggedInstructions()
        {
            var instructions = _context.DraftingInstructions
                .Where(i => i.Status == "Submitted")
                .Select(i => new
                {
                    i.DraftingInstructionID,
                    i.Title,
                    i.Description,
                    i.DepartmentID,
                    i.ReceivedDate,
                    i.Priority
                })
                .ToList();

            return Ok(instructions);
        }

        /// <summary>
        /// PUT /api/RegistryLogging/MarkLogged/{id}
        /// Updates Status to "Logged".
        /// </summary>
        [HttpPut("MarkLogged/{id}")]
        [Authorize]
        public async Task<IActionResult> MarkAsLogged(int id)
        {
            var instruction = await _context.DraftingInstructions.FindAsync(id);
            if (instruction == null)
                return NotFound();

            instruction.Status = "Logged";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Instruction marked as logged." });
        }
    }
}
