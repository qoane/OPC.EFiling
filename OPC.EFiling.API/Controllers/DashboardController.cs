using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Infrastructure.Data;

namespace OPC.EFiling.API.Controllers
{
    /// <summary>
    /// Provides aggregated metrics for the dashboard as well as current user info.
    /// For a production system this would compute counts based on instruction
    /// statuses, registry logs and other domain entities. Here we compute basic
    /// counts from available data so the dashboard widgets show non‑zero values.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns instruction counts aggregated by simple categories. In a full
        /// implementation these would reflect statuses such as Submitted,
        /// Logged, Assigned and Drafting. We use available data as a stand‑in.
        /// </summary>
        [HttpGet("InstructionCounts")]
        public async Task<IActionResult> InstructionCounts()
        {
            /*
             * Compute basic statistics for the dashboard.  In the original
             * implementation the statuses would be stored on the DraftingInstruction
             * entity (e.g. Submitted, Logged, Assigned, Drafting, Completed).
             * Because our prototype does not have explicit status fields the
             * counts are derived heuristically from related entities.  This
             * ensures that the dashboard cards display meaningful non‑zero values
             * when there is activity in the system.
             *
             *  Submitted   – number of draft records that have not yet been
             *                circulated (i.e. zero circulation logs).
             *  Logged      – number of circulation log entries (each represents
             *                an outgoing PDF/email to a ministry).
             *  Assigned    – total number of draft records (reflecting work in
             *                progress assigned to drafters).
             *  Drafting    – number of drafts that have been modified (i.e.
             *                LastModifiedAt set), used here as a proxy for
             *                active drafting.
             *  Completed   – number of circulation responses (i.e. feedback
             *                received from ministries), which indicates drafts
             *                that have gone through at least one review cycle.
             */
            var totalDrafts = await _context.Drafts.CountAsync();
            var draftsNotCirculated = await _context.Drafts
                .CountAsync(d => !d.CirculationLogs.Any());
            var totalCirculations = await _context.CirculationLogs.CountAsync();
            var draftsModified = await _context.Drafts
                .CountAsync(d => d.LastModifiedAt != null);
            var totalResponses = await _context.CirculationResponses.CountAsync();
            var counts = new
            {
                Submitted = draftsNotCirculated,
                Logged = totalCirculations,
                Assigned = totalDrafts,
                Drafting = draftsModified,
                Completed = totalResponses
            };
            return Ok(counts);
        }

        /// <summary>
        /// Returns counts of outstanding tasks and messages for the current user.
        /// Currently this returns dummy data; integration with a task engine would
        /// populate counts based on real assignments and notifications.
        /// </summary>
        [HttpGet("UserCounts")]
        public async Task<IActionResult> UserCounts()
        {
            /*
             * Returns counts of outstanding notifications, messages and tasks
             * for the currently authenticated user.  Notifications are based
             * on unresolved comments across the user’s assignments; tasks are
             * derived from draft assignments depending on the user’s role.  In
             * a full system messages would come from a dedicated messaging
             * table; here we always return zero messages.
             */
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaim, out var userId);
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Determine tasks: count drafts assigned to the current user or
            // relevant to their role.  Registry officers work on drafts
            // without circulation logs; PCs work on drafts that have been
            // logged; drafters work on drafts they created.
            var tasks = 0;
            if (roles.Contains("RegistryOfficer"))
            {
                tasks += await _context.Drafts.CountAsync(d => !d.CirculationLogs.Any());
            }
            if (roles.Contains("PC"))
            {
                tasks += await _context.CirculationLogs.CountAsync();
            }
            if (roles.Contains("Drafter"))
            {
                tasks += await _context.Drafts.CountAsync(d => d.CreatedByUserID == userId);
            }
            if (roles.Contains("Admin"))
            {
                // Admins see all tasks; count drafts not yet completed.
                tasks += await _context.Drafts.CountAsync();
            }

            // Notifications: unresolved comments for instructions associated with the user.
            var notifications = 0;
            if (roles.Contains("Drafter") || roles.Contains("PC") || roles.Contains("RegistryOfficer"))
            {
                // Get instruction IDs from drafts the user is involved with.
                var instructionIds = await _context.Drafts
                    .Where(d => d.CreatedByUserID == userId)
                    .Select(d => d.DraftingInstructionID)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .Distinct()
                    .ToListAsync();
                notifications = await _context.Comments.CountAsync(c => !c.IsResolved && instructionIds.Contains(c.DraftingInstructionID));
            }

            var result = new
            {
                Notifications = notifications,
                Messages = 0,
                Tasks = tasks
            };
            return Ok(result);
        }

        /// <summary>
        /// Returns basic info about the currently authenticated user for greeting
        /// purposes. Includes name and role claims.
        /// </summary>
        [HttpGet("UserInfo")]
        public IActionResult UserInfo()
        {
            var name = User.FindFirstValue("name") ?? User.Identity?.Name ?? "User";
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            return Ok(new { name, roles });
        }
    }
}