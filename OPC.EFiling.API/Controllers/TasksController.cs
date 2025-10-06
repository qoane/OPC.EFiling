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
    /// existing domain entities rather than maintaining a separate task table.
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

        [HttpGet("user")]
        public async Task<IActionResult> GetUserTasks()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaim, out var userId);
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var tasks = new List<object>();

            // ---------------- Drafter Tasks ----------------
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

            // ---------------- Registry Officer Tasks ----------------
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

            // ---------------- PC Tasks ----------------
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

            // ---------------- Senior PC Tasks ----------------
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

            // ---------------- MDA Tasks ----------------
            if (roles.Contains("MDA"))
            {
                // Circulation logs with no recorded responses
                var pendingResponses = await _context.CirculationLogs
                    .Include(cl => cl.Draft)
                    .Where(cl => !_context.CirculationResponses.Any(r => r.CirculationLogId == cl.CirculationLogId))
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

            // ---------------- Admin Tasks ----------------
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

            // ---------------- Sort by Date ----------------
            var ordered = tasks
                .OrderByDescending(t => (DateTime)t.GetType().GetProperty("CreatedAt")!.GetValue(t)!)
                .ToList();

            return Ok(ordered);
        }
    }
}
