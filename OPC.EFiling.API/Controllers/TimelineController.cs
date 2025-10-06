using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Infrastructure.Data;

namespace OPC.EFiling.API.Controllers
{
    /// <summary>
    /// Provides a chronological view of events associated with a drafting
    /// instruction.  The timeline aggregates drafts, comments, circulation
    /// events, responses and signatures to illustrate the lifecycle of a law.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TimelineController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TimelineController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of timeline events for the given drafting instruction
        /// ID. Events are sorted ascending by date. Each event contains a
        /// timestamp, a type string and a description. Clients can use the
        /// type to decide which icon to display.
        /// </summary>
        [HttpGet("{instructionId}")]
        public async Task<IActionResult> Get(int instructionId)
        {
            var events = new List<TimelineEvent>();

            // Draft creation and modifications
            var drafts = await _context.Drafts
                .Where(d => d.DraftingInstructionID == instructionId)
                .ToListAsync();
            foreach (var d in drafts)
            {
                events.Add(new TimelineEvent
                {
                    Date = d.CreatedAt,
                    Type = "DraftCreated",
                    Description = "Draft created"
                });
                if (d.LastModifiedAt.HasValue)
                {
                    events.Add(new TimelineEvent
                    {
                        Date = d.LastModifiedAt.Value,
                        Type = "DraftModified",
                        Description = "Draft modified"
                    });
                }
            }

            // Comments
            var comments = await _context.Comments
                .Where(c => c.DraftingInstructionID == instructionId)
                .ToListAsync();
            foreach (var c in comments)
            {
                events.Add(new TimelineEvent
                {
                    Date = c.CreatedAt,
                    Type = "Comment",
                    Description = $"Comment added by {c.AuthorName}: {c.Text}"
                });
            }

            // Circulations
            var circulations = await _context.CirculationLogs
                .Include(cl => cl.Draft)
                .Where(cl => cl.Draft.DraftingInstructionID == instructionId)
                .ToListAsync();
            foreach (var c in circulations)
            {
                events.Add(new TimelineEvent
                {
                    Date = c.SentAt,
                    Type = "Circulation",
                    Description = $"Draft circulated to {c.SentToEmail}"
                });
            }

            // Responses
            var responses = await _context.CirculationResponses
                .Include(cr => cr.CirculationLog)
                .ThenInclude(cl => cl.Draft)
                .Where(cr => cr.CirculationLog.Draft.DraftingInstructionID == instructionId)
                .ToListAsync();
            foreach (var r in responses)
            {
                events.Add(new TimelineEvent
                {
                    Date = r.ReceivedAt,
                    Type = "Response",
                    Description = "Response received from ministry"
                });
            }

            // Signatures
            var signatures = await _context.Signatures
                .Where(s => s.DraftingInstructionId == instructionId)
                .ToListAsync();
            foreach (var s in signatures)
            {
                events.Add(new TimelineEvent
                {
                    Date = s.SignedAt,
                    Type = "Signature",
                    Description = $"Signature by {s.SignerName}"
                });
            }

            // Sort events by date ascending
            var ordered = events.OrderBy(e => e.Date).ToList();
            return Ok(ordered);
        }

        public class TimelineEvent
        {
            public DateTime Date { get; set; }
            public string Type { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }
    }
}