using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Domain.Entities;
using OPC.EFiling.Infrastructure.Data;

namespace OPC.EFiling.API.Controllers
{
    /// <summary>
    /// REST API controller for managing drafting templates. Templates are stored in the database and the
    /// corresponding files live under <c>wwwroot/templates</c>. Only Admin and PC users are allowed to
    /// create, update or delete templates. Anyone can fetch the list of available templates to use
    /// when creating new instructions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TemplatesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public TemplatesController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        /// <summary>
        /// Returns all available templates. This endpoint is public for authenticated users so they can
        /// select templates when submitting or drafting instructions.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTemplates()
        {
            var templates = await _context.Templates
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return Ok(templates);
        }

        /// <summary>
        /// Uploads a new template. The file is saved to the <c>wwwroot/templates</c> folder and a
        /// database record is created. Only Admin and PC roles can perform this action.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,PC")]
        public async Task<IActionResult> Create([FromForm] TemplateUploadDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required.");

            if (dto.File == null || dto.File.Length == 0)
            {
                return BadRequest("A template file must be provided.");
            }

            // Ensure the templates directory exists
            var templatesDir = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "templates");
            Directory.CreateDirectory(templatesDir);

            // Generate a unique filename preserving extension
            var fileExt = Path.GetExtension(dto.File.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExt}";
            var filePath = Path.Combine(templatesDir, fileName);

            // Save the file
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var template = new Template
            {
                Name = dto.Name,
                Description = dto.Description,
                FilePath = Path.Combine("templates", fileName).Replace(Path.DirectorySeparatorChar, '/'),
                CreatedAt = DateTime.UtcNow
            };

            _context.Templates.Add(template);
            await _context.SaveChangesAsync();

            return Ok(template);
        }

        /// <summary>
        /// Updates an existing template. You can change the name, description and optionally
        /// replace the underlying file. Only Admin and PC roles can perform this action.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,PC")]
        public async Task<IActionResult> Update(int id, [FromForm] TemplateUploadDto dto)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Name)) template.Name = dto.Name;
            template.Description = dto.Description;

            if (dto.File != null && dto.File.Length > 0)
            {
                // Determine current file path on disk
                var currentPath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), template.FilePath.Replace('/', Path.DirectorySeparatorChar));
                // Delete current file if it exists
                if (System.IO.File.Exists(currentPath)) System.IO.File.Delete(currentPath);

                // Save new file
                var templatesDir = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "templates");
                Directory.CreateDirectory(templatesDir);
                var newExt = Path.GetExtension(dto.File.FileName);
                var newName = $"{Guid.NewGuid()}{newExt}";
                var newPath = Path.Combine(templatesDir, newName);
                await using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }
                template.FilePath = Path.Combine("templates", newName).Replace(Path.DirectorySeparatorChar, '/');
            }

            await _context.SaveChangesAsync();
            return Ok(template);
        }

        /// <summary>
        /// Deletes a template and removes its file from disk. Only Admin and PC roles can perform this action.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,PC")]
        public async Task<IActionResult> Delete(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null) return NotFound();

            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();

            // Remove the file from disk after DB changes
            var absolutePath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), template.FilePath.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(absolutePath)) System.IO.File.Delete(absolutePath);

            return Ok(new { message = "Template deleted." });
        }

        /// <summary>
        /// Data transfer object used for uploading and updating templates. It binds form fields and the uploaded file
        /// using [FromForm] in controller actions.
        /// </summary>
        public class TemplateUploadDto
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public IFormFile? File { get; set; }
        }
    }
}