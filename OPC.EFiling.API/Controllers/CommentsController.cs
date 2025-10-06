using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Domain.Entities;
using OPC.EFiling.Infrastructure.Data;

namespace OPC.EFiling.API.Controllers
{
    /// <summary>
    /// API controller for managing comments attached to drafting instructions. All endpoints
    /// require authentication. Comments can be created, replied to, resolved and listed.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns all comments associated with a drafting instruction. Comments are ordered by
        /// creation time descending and include nested replies. Resolved comments are included
        /// so reviewers have full visibility.
        /// </summary>
        /// <param name="instructionId">The ID of the drafting instruction</param>
        [HttpGet("{instructionId}")]
        public async Task<IActionResult> GetComments(int instructionId)
        {
            var comments = await _context.Comments
                .Where(c => c.DraftingInstructionID == instructionId && c.ParentCommentId == null)
                .Include(c => c.Replies)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return Ok(comments);
        }

        /// <summary>
        /// Creates a new comment on a drafting instruction. The current user's name is captured
        /// from the JWT claims and stored as the author. The request body should contain
        /// DraftingInstructionID, Text and optionally Selection.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] Comment comment)
        {
            if (comment == null || string.IsNullOrWhiteSpace(comment.Text))
            {
                return BadRequest("Comment must include text.");
            }
            comment.AuthorName = User.FindFirstValue("name") ?? User.Identity?.Name ?? "Unknown";
            comment.CreatedAt = DateTime.UtcNow;
            comment.IsResolved = false;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return Ok(comment);
        }

        /// <summary>
        /// Adds a reply to an existing comment. The parent comment ID is provided in the URL.
        /// The body should contain DraftingInstructionID and Text. AuthorName and CreatedAt
        /// are set automatically from the current user.
        /// </summary>
        [HttpPost("{commentId}/reply")]
        public async Task<IActionResult> AddReply(int commentId, [FromBody] Comment reply)
        {
            var parent = await _context.Comments.FindAsync(commentId);
            if (parent == null)
            {
                return NotFound();
            }
            reply.ParentCommentId = commentId;
            reply.DraftingInstructionID = parent.DraftingInstructionID;
            reply.AuthorName = User.FindFirstValue("name") ?? User.Identity?.Name ?? "Unknown";
            reply.CreatedAt = DateTime.UtcNow;
            _context.Comments.Add(reply);
            await _context.SaveChangesAsync();
            return Ok(reply);
        }

        /// <summary>
        /// Marks a comment and its thread as resolved. Once resolved the front‑end can
        /// display the comment as collapsed or at the bottom of the list. The comment
        /// remains in the database for audit purposes.
        /// </summary>
        [HttpPut("{commentId}/resolve")]
        public async Task<IActionResult> Resolve(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }
            comment.IsResolved = true;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}