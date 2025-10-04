using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OPC.EFiling.Application.DTOs;
using OPC.EFiling.Application.Services;
using OPC.EFiling.Domain.Entities;
using OPC.EFiling.Infrastructure.Data;

namespace OPC.EFiling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // tighten to a role (e.g., Registry) if needed
    public class CirculationController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPdfExportService _pdf;
        private readonly IEmailService _email;
        private readonly IWebHostEnvironment _env;

        public CirculationController(
            AppDbContext db,
            IPdfExportService pdf,
            IEmailService email,
            IWebHostEnvironment env)
        {
            _db = db;
            _pdf = pdf;
            _email = email;
            _env = env;
        }

        /// <summary>
        /// Generate a PDF of the draft and email it to the requesting ministry.
        /// Persists a Document + UploadedFile record for audit.
        /// </summary>
        [HttpPost("send/{draftId:int}")]
        public async Task<IActionResult> SendToMinistry([FromRoute] int draftId, [FromBody] SendDraftRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ToEmail))
                return BadRequest("ToEmail is required.");

            var draft = await _db.Drafts
                .Include(d => d.DraftingInstruction)
                .FirstOrDefaultAsync(d => d.DraftID == draftId);

            if (draft is null)
                return NotFound("Draft not found.");

            // 1) Render PDF in-memory
            var versionLabel = string.IsNullOrWhiteSpace(dto.VersionLabel) ? null : dto.VersionLabel!.Trim();
            var pdfBytes = await _pdf.RenderDraftPdfAsync(draft, versionLabel);
            var fileName = $"Draft_{draft.DraftID}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

            // 2) Create Document metadata (linked to the instruction)
            var userId = GetCurrentUserIdOrNull() ?? 0;
            var docTitle = string.IsNullOrWhiteSpace(draft.DraftingInstruction?.Title)
                ? "Draft PDF"
                : draft.DraftingInstruction!.Title!;

            var document = new Document
            {
                InstructionID = draft.DraftingInstructionID ?? 0,
                Title = string.IsNullOrWhiteSpace(versionLabel) ? docTitle : $"{docTitle} ({versionLabel})",
                Version = 1,               // bump if you build proper versioning
                Status = "Sent",
                CreatedBy = userId,
                DateCreated = DateTime.UtcNow
            };
            _db.Documents.Add(document);
            await _db.SaveChangesAsync();       // materialize DocumentID

            // 3) Persist file to disk and create UploadedFile pointer
            var outboxRoot = Path.Combine(_env.ContentRootPath, "App_Data", "Outbox");
            Directory.CreateDirectory(outboxRoot);

            var fullPath = Path.Combine(outboxRoot, fileName);
            await System.IO.File.WriteAllBytesAsync(fullPath, pdfBytes);

            var uploaded = new UploadedFile
            {
                DocumentID = document.DocumentID,
                FileName = fileName,
                FilePath = fullPath,
                FileType = "application/pdf",
                UploadedBy = userId,
                DateUploaded = DateTime.UtcNow
            };
            _db.Set<UploadedFile>().Add(uploaded);
            await _db.SaveChangesAsync();

            // 4) Email with attachment
            var subject = string.IsNullOrWhiteSpace(dto.Subject)
                ? $"OPC Draft {versionLabel ?? ""} — Ref #{draft.DraftingInstructionID}"
                : dto.Subject;

            var bodyHtml = dto.MessageHtml ?? $@"
                <p>Dear Colleagues,</p>
                <p>Please find attached the draft ({versionLabel ?? "unversioned"}) for your review.</p>
                <p>Reference: <b>{draft.DraftingInstructionID}</b></p>
                <p>Regards,<br/>Office of Parliamentary Counsel</p>";

            await _email.SendEmailAsync(
                to: dto.ToEmail,
                subject: subject,
                body: bodyHtml,
                isHtml: true,
                cc: dto.CcEmail,
                attachmentBytes: pdfBytes,
                attachmentName: fileName,
                attachmentMime: "application/pdf"
            );

            return Ok(new
            {
                message = "Draft sent successfully.",
                draftId = draft.DraftID,
                documentId = document.DocumentID,
                uploadedFileId = uploaded.UploadedFileID,
                path = uploaded.FilePath
            });
        }

        /// <summary>
        /// Simple history view by instruction: last file per Document.
        /// </summary>
        [HttpGet("history/by-instruction/{instructionId:int}")]
        public async Task<IActionResult> GetHistoryByInstruction(int instructionId)
        {
            var records = await _db.Documents
                .Where(d => d.InstructionID == instructionId)
                .OrderByDescending(d => d.DateCreated)
                .Select(d => new
                {
                    d.DocumentID,
                    d.Title,
                    d.Version,
                    d.Status,
                    d.CreatedBy,
                    d.DateCreated,
                    File = _db.Set<UploadedFile>()
                              .Where(f => f.DocumentID == d.DocumentID)
                              .OrderByDescending(f => f.DateUploaded)
                              .Select(f => new { f.UploadedFileID, f.FileName, f.FilePath, f.FileType, f.DateUploaded })
                              .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(records);
        }

        private int? GetCurrentUserIdOrNull()
        {
            var claim = User?.Claims?.FirstOrDefault(c => c.Type == "sub" || c.Type.EndsWith("/nameidentifier"));
            if (claim != null && int.TryParse(claim.Value, out var id)) return id;
            return null;
        }
    }
}
