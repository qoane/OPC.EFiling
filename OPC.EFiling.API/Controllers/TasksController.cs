using System;
using System.Collections.Generic;
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
    /// Provides task lists for users based on their role and the state of drafts and circulations.
    /// Tasks represent work items such as drafting an instruction, logging a draft, reviewing a
    /// circulated document or responding to a ministry.  This controller aggregates data from
    /// existing domain entities rather than maintaining a separate task table.  Additional task
    /// types can be added as the workflow expands (e.g. admin approvals, user onboarding).
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of pending tasks for the currently authenticated user.  The tasks
        /// are computed on the fly based on the user's roles and the current state of
        /// drafts, circulations and comments.  Each task object includes the
        /// drafting instruction identifier (if available), a title, a description, a
        /// task type, a status and the date it was created.  Clients can display this
        /// information in a task dashboard.
        /// </summary>
        [HttpGet("user")]
        public async Task<IActionResult> GetUserTasks()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaim, out var userId);
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var tasks = new List<object>();

            // Drafter tasks: all drafts created by this user.  A draft is considered a task
            // until at least one circulation log exists.  We don't rely on a separate
            // status field so this heuristic keeps the drafter's task list populated.
            if (roles.Contains("Drafter"))
            {
                var myDrafts = await _context.Drafts
                    .Where(d => d.CreatedByUserID == userId)
                    .Include(d => d.CirculationLogs)
                    .ToListAsync();
                foreach (var d in myDrafts)
                {
                    var status = d.CirculationLogs.Any() ? "Sent" : "Pending";
                    tasks.Add(new
                    {
                        DraftingInstructionID = d.DraftingInstructionID,
                        Title = $"Draft #{d.DraftID}",
                        Description = "Draft assigned to you",
                        TaskType = "Draft",
                        Status = status,
                        CreatedAt = d.CreatedAt
                    });
                }
            }

            // Registry officer tasks: drafts that have not been circulated (no circulation logs)
            // and therefore need to be logged and sent out.  Each such draft becomes a task.
            if (roles.Contains("RegistryOfficer"))
            {
                var unlogged = await _context.Drafts
                    .Where(d => !d.CirculationLogs.Any())
                    .ToListAsync();
                foreach (var d in unlogged)
                {
                    tasks.Add(new
                    {
                        DraftingInstructionID = d.DraftingInstructionID,
                        Title = $"Draft #{d.DraftID}",
                        Description = "Draft awaiting registry logging",
                        TaskType = "Registry Logging",
                        Status = "Unlogged",
                        CreatedAt = d.CreatedAt
                    });
                }
            }

            // PC tasks: drafts that have at least one circulation log.  PCs review
            // circulated drafts and manage allocations to drafters.  Each such draft is
            // presented as a review task.
            if (roles.Contains("PC"))
            {
                var loggedDrafts = await _context.Drafts
                    .Where(d => d.CirculationLogs.Any())
                    .Include(d => d.CirculationLogs)
                    .ToListAsync();
                foreach (var d in loggedDrafts)
                {
                    tasks.Add(new
                    {
                        DraftingInstructionID = d.DraftingInstructionID,
                        Title = $"Draft #{d.DraftID}",
                        Description = "Draft awaiting PC review",
                        TaskType = "PC Review",
                        Status = "Logged",
                        CreatedAt = d.CreatedAt
                    });
                }
            }

            // Senior PC tasks: drafts that have been circulated and require final approval.
            // We approximate by showing all drafts with circulation logs.  Additional
            // filtering could be implemented (e.g. only drafts without signatures).
            if (roles.Contains("SeniorPC"))
            {
                var approvalDrafts = await _context.Drafts
                    .Where(d => d.CirculationLogs.Any())
                    .ToListAsync();
                foreach (var d in approvalDrafts)
                {
                    tasks.Add(new
                    {
                        DraftingInstructionID = d.DraftingInstructionID,
                        Title = $"Draft #{d.DraftID}",
                        Description = "Draft awaiting final approval",
                        TaskType = "Approval",
                        Status = "Pending",
                        CreatedAt = d.CreatedAt
                    });
                }
            }

            // MDA tasks: circulations with no responses yet.  Ministries need to
            // review the circulated draft and send back their response.  Each
            // unresponded circulation is shown as a task.
            if (roles.Contains("MDA"))
            {
                var pendingResponses = await _context.CirculationLogs
                    .Include(cl => cl.Draft)
                    .Include(cl => cl.Responses)
                    .Where(cl => !cl.Responses.Any())
                    .ToListAsync();
                foreach (var c in pendingResponses)
                {
                    tasks.Add(new
                    {
                        DraftingInstructionID = c.Draft.DraftingInstructionID,
                        Title = $"Draft #{c.Draft.DraftID}",
                        Description = $"Response required for email sent to {c.SentToEmail}",
                        TaskType = "Ministry Response",
                        Status = "Awaiting Response",
                        CreatedAt = c.SentAt
                    });
                }
            }

            // Admin tasks: unresolved comments across all instructions.  Admins can
            // monitor outstanding discussions and assign follow‑ups.  Each unresolved
            // comment becomes a task.
            if (roles.Contains("Admin"))
            {
                var unresolved = await _context.Comments
                    .Where(c => !c.IsResolved)
                    .ToListAsync();
                foreach (var c in unresolved)
                {
                    tasks.Add(new
                    {
                        DraftingInstructionID = c.DraftingInstructionID,
                        Title = $"Comment #{c.CommentId}",
                        Description = c.Text,
                        TaskType = "Admin Comment",
                        Status = "Unresolved",
                        CreatedAt = c.CreatedAt
                    });
                }
            }

            // Sort tasks by creation date (descending) for display
            var ordered = tasks
                .OrderByDescending(t => (DateTime)t.GetType().GetProperty("CreatedAt")!.GetValue(t)!)
                .ToList();
            return Ok(ordered);
        }
    }
}