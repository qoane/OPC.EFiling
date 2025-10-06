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
    /// API controller for capturing and retrieving signatures on drafting
    /// instructions. Signatures are captured from a canvas on the front‑end as
    /// base64 encoded PNG images.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SignaturesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public SignaturesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns all signatures associated with a drafting instruction.
        /// </summary>
        [HttpGet("{instructionId}")]
        public async Task<IActionResult> Get(int instructionId)
        {
            var list = await _context.Signatures
                .Where(s => s.DraftingInstructionId == instructionId)
                .OrderBy(s => s.SignedAt)
                .ToListAsync();
            return Ok(list);
        }

        /// <summary>
        /// Saves a new signature for the given drafting instruction.  The body
        /// should provide ImageData (base64 PNG) and optionally SignerName.  If
        /// SignerName is omitted the current user's name claim is used.
        /// </summary>
        [HttpPost("{instructionId}")]
        public async Task<IActionResult> Post(int instructionId, [FromBody] SignatureDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ImageData))
                return BadRequest("ImageData is required");
            var name = !string.IsNullOrWhiteSpace(dto.SignerName)
                ? dto.SignerName
                : (User.FindFirstValue("name") ?? User.Identity?.Name ?? "Unknown");
            var signature = new Signature
            {
                DraftingInstructionId = instructionId,
                SignerName = name,
                ImageData = dto.ImageData,
                SignedAt = DateTime.UtcNow
            };
            _context.Signatures.Add(signature);
            await _context.SaveChangesAsync();
            return Ok(signature);
        }

        public class SignatureDto
        {
            public string ImageData { get; set; } = string.Empty;
            public string? SignerName { get; set; }
        }
    }
}