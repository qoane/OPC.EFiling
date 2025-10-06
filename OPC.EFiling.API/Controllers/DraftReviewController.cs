using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Infrastructure.Data;

namespace OPC.EFiling.API.Controllers
{
    /// <summary>
    /// Controller for senior PC review actions such as approving a draft or requesting
    /// revisions. These endpoints are placeholders; in a full system they would update
    /// instruction status and notify the drafter accordingly.
    /// </summary>
    [ApiController]
    [Route("api/drafts")] // reuse base path
    [Authorize(Roles = "SeniorPC,Admin")]
    public class DraftReviewController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DraftReviewController(AppDbContext context) { _context = context; }

        /// <summary>
        /// Approves the current draft for a given instruction. In this placeholder
        /// implementation it simply returns success. A production implementation
        /// would update the instruction status to Approved and notify the drafter.
        /// </summary>
        [HttpPost("approve/{instructionId}")]
        public async Task<IActionResult> Approve(int instructionId)
        {
            // TODO: update instruction status and create audit log entry.
            await Task.CompletedTask;
            return Ok(new { message = "Draft approved" });
        }

        /// <summary>
        /// Requests revisions for the current draft. Accepts a message body with
        /// reviewer feedback. In this placeholder it simply returns success. A
        /// production implementation would create a task for the drafter and
        /// update status to RevisionRequested.
        /// </summary>
        [HttpPost("request/{instructionId}")]
        public async Task<IActionResult> RequestChanges(int instructionId, [FromBody] ReviewRequest request)
        {
            // TODO: create a revision request entry and notify drafter.
            await Task.CompletedTask;
            return Ok(new { message = "Revisions requested", feedback = request?.Message ?? string.Empty });
        }

        public class ReviewRequest
        {
            public string? Message { get; set; }
        }
    }
}