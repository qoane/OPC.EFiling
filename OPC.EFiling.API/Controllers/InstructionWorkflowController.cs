using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Application.Services;
using OPC.EFiling.Domain.Entities;
using OPC.EFiling.Infrastructure.Data;
using System.Threading.Tasks;
using System.Net.Mail;

namespace OPC.EFiling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstructionWorkflowControllerWithNotification : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public InstructionWorkflowControllerWithNotification(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("AssignToPc")]
        [Authorize(Roles = "RegistryOfficer")]
        public async Task<IActionResult> AssignToPc([FromBody] AssignInstructionDto dto)
        {
            var instruction = await _context.DraftingInstructions.FindAsync(dto.InstructionId);
            if (instruction == null)
                return NotFound("Instruction not found.");

            var pcUser = await _context.Users.FindAsync(dto.UserId);
            if (pcUser == null || string.IsNullOrEmpty(pcUser.Email))
                return NotFound("PC user not found or email is missing.");

            instruction.AssignedDrafterID = dto.UserId;
            instruction.Status = "PC Assigned";
            await _context.SaveChangesAsync();

            string subject = "New Drafting Instruction Assigned";
            string body = $@"
                <p>Dear <strong>{pcUser.FullName}</strong>,</p>
                <p>A new drafting instruction titled <strong>{instruction.Title}</strong> has been assigned to you.</p>
                <p>Please <a href='https://efiling.opc.gov.ls'>log in</a> to the system to begin work.</p>
                <br />
                <p>Regards,<br/><strong>OPC E‑Filing System</strong></p>
            ";

            await _emailService.SendEmailAsync(pcUser.Email, subject, body, isHtml: true, cc: "sqoane@gmail.com");

            return Ok(new { message = "Instruction assigned to PC successfully." });
        }

        [HttpPost("AssignToDrafter")]
        [Authorize(Roles = "PC")]
        public async Task<IActionResult> AssignToDrafter([FromBody] AssignInstructionDto dto)
        {
            var instruction = await _context.DraftingInstructions.FindAsync(dto.InstructionId);
            if (instruction == null)
                return NotFound("Instruction not found.");

            var drafter = await _context.Users.FindAsync(dto.UserId);
            if (drafter == null || string.IsNullOrEmpty(drafter.Email))
                return NotFound("Drafter not found or email is missing.");

            instruction.AssignedDrafterID = dto.UserId;
            instruction.Status = "Assigned";
            await _context.SaveChangesAsync();

            string subject = "Instruction Reassigned to You";
            string body = $@"
                <p>Dear <strong>{drafter.FullName}</strong>,</p>
                <p>You’ve been reassigned the instruction titled <strong>{instruction.Title}</strong>.</p>
                <p>Please take note and begin drafting as necessary.</p>
                <br />
                <p>Regards,<br/><strong>OPC E‑Filing System</strong></p>
            ";

            await _emailService.SendEmailAsync(drafter.Email, subject, body, isHtml: true, cc: "sqoane@gmail.com");

            return Ok(new { message = "Instruction assigned to drafter successfully." });
        }

        public class AssignInstructionDto
        {
            public int InstructionId { get; set; }
            public int UserId { get; set; }
        }
    }
}
