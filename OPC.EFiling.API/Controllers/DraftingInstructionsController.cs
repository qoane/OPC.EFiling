using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;      // for IWebHostEnvironment
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPC.EFiling.Domain.Entities;
using OPC.EFiling.Infrastructure.Data;

namespace OPC.EFiling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DraftingInstructionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DraftingInstructionsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /// <summary>
        /// GET /api/DraftingInstructions
        /// Returns all instructions (including Description + attachments).
        /// </summary>
        [HttpGet]
        [Authorize]
        public IActionResult GetInstructions()
        {
            var instructions = _context.DraftingInstructions
                .Select(i => new
                {
                    i.DraftingInstructionID,
                    i.Title,
                    i.Description,
                    i.DepartmentID,
                    i.AssignedDrafterID,
                    i.Status,
                    i.Priority,
                    i.ReceivedDate,
                    Files = _context.InstructionAttachments
                                .Where(f => f.DraftingInstructionID == i.DraftingInstructionID)
                                .Select(f => new { f.InstructionAttachmentID, f.FileName, f.FilePath })
                                .ToList()
                })
                .ToList();

            return Ok(instructions);
        }

        /// <summary>
        /// GET /api/DraftingInstructions/{id}
        /// Returns a single instruction by ID (with Description + attachments).
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize]
        public IActionResult GetInstructionById(int id)
        {
            var instr = _context.DraftingInstructions
                .Where(i => i.DraftingInstructionID == id)
                .Select(i => new
                {
                    i.DraftingInstructionID,
                    i.Title,
                    i.Description,
                    i.DepartmentID,
                    i.AssignedDrafterID,
                    i.Status,
                    i.Priority,
                    i.ReceivedDate,
                    Files = _context.InstructionAttachments
                                .Where(f => f.DraftingInstructionID == i.DraftingInstructionID)
                                .Select(f => new { f.InstructionAttachmentID, f.FileName, f.FilePath })
                                .ToList()
                })
                .FirstOrDefault();

            if (instr == null)
                return NotFound();

            return Ok(instr);
        }

        /// <summary>
        /// POST /api/DraftingInstructions
        /// Accepts form-data: Title, Description, DepartmentID, AssignedDrafterID, Status, Priority, ReceivedDate, Files[]
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateInstruction([FromForm] CreateInstructionDto dto)
        {
            // 1) Get current user ID from JWT (ClaimTypes.NameIdentifier).
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            // 2) Create DraftingInstruction entity, including Description
            var instruction = new DraftingInstruction
            {
                Title = dto.Title,
                Description = dto.Description,
                DepartmentID = dto.DepartmentID,
                AssignedDrafterID = dto.AssignedDrafterID,
                Status = dto.Status,
                Priority = dto.Priority,
                ReceivedDate = dto.ReceivedDate
            };

            _context.DraftingInstructions.Add(instruction);
            await _context.SaveChangesAsync(); // Now instruction.DraftingInstructionID is populated

            // 3) If any files were uploaded, save to disk + create InstructionAttachment rows
            if (dto.Files != null && dto.Files.Count > 0)
            {
                // Ensure the folder wwwroot/uploads exists
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                foreach (var file in dto.Files)
                {
                    if (file.Length > 0)
                    {
                        // Create a unique filename to avoid collisions
                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Save file to disk
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Record attachment in database (FilePath is relative to wwwroot)
                        var attachment = new InstructionAttachment
                        {
                            DraftingInstructionID = instruction.DraftingInstructionID,
                            FileName = file.FileName,
                            FilePath = Path.Combine("uploads", uniqueFileName).Replace("\\", "/")
                        };
                        _context.InstructionAttachments.Add(attachment);
                    }
                }

                await _context.SaveChangesAsync();
            }

            // 4) Return 201 Created with the new ID
            return CreatedAtAction(
                nameof(GetInstructionById),
                new { id = instruction.DraftingInstructionID },
                new { instruction.DraftingInstructionID, instruction.Title, instruction.Description }
            );
        }
    }


}
