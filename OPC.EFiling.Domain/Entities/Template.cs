using System;

namespace OPC.EFiling.Domain.Entities
{
    /// <summary>
    /// Represents a drafting template stored in the system. Templates can be reused as starting points for new
    /// drafting instructions. Each template has a name, optional description and a file path on disk. Templates are
    /// stored in the database so they can be managed via the API and front‑end.
    /// </summary>
    public class Template
    {
        public int TemplateId { get; set; }

        /// <summary>
        /// Human friendly name of the template.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description to help users understand the purpose or contents of the template.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Relative file path of the template within the wwwroot/templates folder. This path is used to serve
        /// the template file through static file middleware or download endpoints.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the template was created. Defaults to the current UTC time when the entity is persisted.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}