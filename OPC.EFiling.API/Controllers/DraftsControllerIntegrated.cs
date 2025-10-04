using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Domain.Entities;
using OPC.EFiling.Infrastructure.Data;
using OPC.EFiling.Application.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OPC.EFiling.API.Controllers
{
    /// <summary>
    /// A refactored version of the DraftsController that integrates the DraftLockService
    /// and records a new draft version on every save or submit.  This controller
    /// should replace your existing DraftsController once the DraftLockService is
    /// registered in the DI container.  It also includes TODO hooks for
    /// notifying the next user in the workflow.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DraftsControllerIntegrated : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly DraftLockService _lockService;

        public DraftsControllerIntegrated(AppDbContext context, DraftLockService lockService)
        {
            _context = context;
            _lockService = lockService;
        }

        /// <summary>
        /// Returns all instructions assigned to the logged‑in drafter.
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
        /// Submits a completed draft.  This endpoint acquires a lock, creates
        /// a new draft version, updates the status, saves the changes and releases
        /// the lock.  It is equivalent to "check in" in a document management system.
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

            // Acquire lock; if someone else holds it, deny submission
            var lockAcquired = await _lockService.AcquireLockAsync(dto.DraftingInstructionID, drafterId);
            if (!lockAcquired)
                return Conflict("Instruction is currently locked by another user.");

            // Always create a new draft record for this submission
            var draft = new Draft
            {
                DraftingInstructionID = dto.DraftingInstructionID,
                ContentHtml = dto.HtmlContent ?? string.Empty, // Fix for CS8601
                CreatedByUserID = drafterId,
                CreatedAt = DateTime.UtcNow
            };

            instruction.Status = "Draft Submitted";
            _context.Drafts.Add(draft);
            await _context.SaveChangesAsync();

            // Release the lock once submission completes
            await _lockService.ReleaseLockAsync(dto.DraftingInstructionID, drafterId);

            // TODO: send notification to the next person in the workflow (e.g. Registrar or PC)

            return Ok(new { message = "Draft submitted successfully." });
        }

        /// <summary>
        /// Returns the latest draft for editing.  If another user holds the lock,
        /// this endpoint returns a 409 Conflict.
        /// </summary>
        [HttpGet("ByInstruction/{id}")]
        [Authorize]
        public async Task<IActionResult> GetDraftByInstruction(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int drafterId))
                return Unauthorized();

            // Acquire lock before allowing edit
            var lockAcquired = await _lockService.AcquireLockAsync(id, drafterId);
            if (!lockAcquired)
                return Conflict("Instruction is currently locked by another user.");

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
        /// Saves a draft without submitting it.  A new draft record is created
        /// each time, preserving a version history.  The lock remains in place
        /// so the user can continue editing.
        /// </summary>
        [HttpPost("Save")]
        [Authorize]
        public async Task<IActionResult> SaveDraft([FromBody] DraftSubmissionDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int drafterId))
                return Unauthorized();

            // Acquire lock before saving
            var lockAcquired = await _lockService.AcquireLockAsync(dto.DraftingInstructionID, drafterId);
            if (!lockAcquired)
                return Conflict("Instruction is currently locked by another user.");

            // Always create a new draft record to preserve version history
            var newDraft = new Draft
            {
                DraftingInstructionID = dto.DraftingInstructionID,
                ContentHtml = dto.HtmlContent ?? string.Empty, // Fix for CS8601
                CreatedByUserID = drafterId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Drafts.Add(newDraft);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Draft saved successfully." });
        }

        /// <summary>
        /// Returns all drafting instructions.  Useful for administrative views.
        /// </summary>
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

        /// <summary>
        /// Payload for saving or submitting a draft.
        /// </summary>
        public class DraftSubmissionDto
        {
            public int DraftingInstructionID { get; set; }
            public string? HtmlContent { get; set; }
        }
    }
}