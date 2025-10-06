using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Infrastructure.Data;

namespace OPC.EFiling.API.Controllers
{
    /// <summary>
    /// API controller that returns side‑by‑side comparisons of the two most recent drafts
    /// for a given instruction. This can be used by reviewers to visualise changes
    /// between versions. Draft history is determined by sorting drafts by their
    /// last modified or created timestamp.
    /// </summary>
    [ApiController]
    [Route("api/drafts/compare")]
    [Authorize]
    public class DraftComparisonController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DraftComparisonController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns the two most recent drafts for an instruction. If there is only one draft
        /// available the old version will be returned as null. This endpoint returns plain
        /// HTML strings for each draft version; the client is responsible for generating
        /// a diff.
        /// </summary>
        [HttpGet("{instructionId}")]
        public async Task<IActionResult> CompareDrafts(int instructionId)
        {
            var drafts = await _context.Drafts
                .Where(d => d.DraftingInstructionID == instructionId)
                .OrderByDescending(d => d.LastModifiedAt ?? d.CreatedAt)
                .Take(2)
                .Select(d => d.ContentHtml)
                .ToListAsync();

            string? newHtml = drafts.Count > 0 ? drafts[0] : null;
            string? oldHtml = drafts.Count > 1 ? drafts[1] : null;

            return Ok(new { oldHtml, newHtml });
        }
    }
}