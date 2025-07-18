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
    public class DraftsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DraftsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/Drafts/ByDrafter
        /// Returns all instructions assigned to the logged-in drafter (for drafting).
        /// </summary>
        [HttpGet("ByDrafter")]
        [Authorize]
        public IActionResult GetAssignedInstructions()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int drafterId))
                return Unauthorized();

            var assigned = _context.DraftingInstructions
                .Where(d => d.AssignedDrafterID == drafterId)
                .Select(d => new
                {
                    d.DraftingInstructionID,
                    d.Title,
                    d.Status,
                    d.ReceivedDate
                })
                .ToList();

            return Ok(assigned);
        }

        /// <summary>
        /// POST /api/Drafts/Submit
        /// Submits a completed draft (CKEditor content) against a drafting instruction.
        /// </summary>
        [HttpPost("Submit")]
        [Authorize]
        public async Task<IActionResult> SubmitDraft([FromBody] DraftSubmissionDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int drafterId))
                return Unauthorized();

            var instruction = await _context.DraftingInstructions
                .FirstOrDefaultAsync(i => i.DraftingInstructionID == dto.DraftingInstructionID);

            if (instruction == null)
                return NotFound("Instruction not found");

            var draft = new Draft
            {
                DraftingInstructionID = dto.DraftingInstructionID,
                ContentHtml = dto.HtmlContent,
                CreatedByUserID = drafterId,
                CreatedAt = DateTime.UtcNow // optional, can be removed
            };

            instruction.Status = "Draft Submitted";

            _context.Drafts.Add(draft);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Draft submitted successfully." });
        }

        /// <summary>
        /// GET /api/Drafts/ByInstruction/{id}
        /// Returns the saved draft for the logged-in drafter & instruction, if any.
        /// </summary>
        [HttpGet("ByInstruction/{id}")]
        [Authorize]
        public async Task<IActionResult> GetDraftByInstruction(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int drafterId))
                return Unauthorized();

            var draft = await _context.Drafts
                .Where(d => d.DraftingInstructionID == id && d.CreatedByUserID == drafterId)
                .OrderByDescending(d => d.LastModifiedAt ?? d.CreatedAt)
                .FirstOrDefaultAsync();

            if (draft == null)
                return NotFound();

            return Ok(new
            {
                draft.DraftID,
                draft.ContentHtml,
                draft.LastModifiedAt
            });
        }

        /// <summary>
        /// POST /api/Drafts/Save
        /// Saves or updates a draft without submitting it
        /// </summary>
        [HttpPost("Save")]
        [Authorize]
        public async Task<IActionResult> SaveDraft([FromBody] DraftSubmissionDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int drafterId))
                return Unauthorized();

            var existing = await _context.Drafts
                .FirstOrDefaultAsync(d =>
                    d.DraftingInstructionID == dto.DraftingInstructionID &&
                    d.CreatedByUserID == drafterId);

            if (existing != null)
            {
                existing.ContentHtml = dto.HtmlContent;
                existing.LastModifiedAt = DateTime.UtcNow;
            }
            else
            {
                var newDraft = new Draft
                {
                    DraftingInstructionID = dto.DraftingInstructionID,
                    ContentHtml = dto.HtmlContent,
                    CreatedByUserID = drafterId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Drafts.Add(newDraft);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Draft saved successfully." });
        }

        [HttpGet("All")]
        [Authorize]
        public IActionResult GetAllInstructions()
        {
            var instructions = _context.DraftingInstructions
                .Select(d => new
                {
                    d.DraftingInstructionID,
                    d.Title,
                    d.Status,
                    d.ReceivedDate
                })
                .ToList();

            return Ok(instructions);
        }

        public class DraftSubmissionDto
        {
            public int DraftingInstructionID { get; set; }
            public string HtmlContent { get; set; }
        }
    }
}
