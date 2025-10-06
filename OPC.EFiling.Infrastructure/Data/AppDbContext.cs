using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Domain.Entities;
using OPC.EFiling.Infrastructure.Data.Configurations;

namespace OPC.EFiling.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Department> Departments { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }

        // Existing sets
        public DbSet<DraftingInstruction> DraftingInstructions { get; set; }
        public DbSet<InstructionAttachment> InstructionAttachments { get; set; }
        public DbSet<Draft> Drafts { get; set; }

        public DbSet<Document> Documents { get; set; }
        public DbSet<UploadedFile> Files { get; set; }
        public DbSet<ApprovalLog> ApprovalLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<InstructionLock> InstructionLocks { get; set; }
        public DbSet<DraftVersion> DraftVersions { get; set; }

        // Circulation-related sets
        /// <summary>
        /// Table storing each time a draft is circulated to a ministry.
        /// </summary>
        public DbSet<CirculationLog> CirculationLogs { get; set; }

        /// <summary>
        /// Table storing responses from ministries for each circulation log entry.
        /// </summary>
        public DbSet<CirculationResponse> CirculationResponses { get; set; }

        // Template sets
        /// <summary>
        /// Stores drafting templates uploaded by administrators or parliamentary counsel. Templates provide starting
        /// documents for new instructions.
        /// </summary>
        public DbSet<Template> Templates { get; set; }

        /// <summary>
        /// Stores threaded comments on drafting instructions. Comments persist across draft versions
        /// and support replies for conversation-like review threads.
        /// </summary>
        public DbSet<Comment> Comments { get; set; }

        /// <summary>
        /// Stores captured signatures for final approval. Each signature is
        /// attached to a drafting instruction and includes the signer's name
        /// and a base64 encoded image.
        /// </summary>
        public DbSet<Signature> Signatures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        base.OnModelCreating(modelBuilder);

            // ── One-to-many: DraftingInstruction → InstructionAttachment
            modelBuilder.Entity<InstructionAttachment>()
                .HasOne(a => a.DraftingInstruction)
                .WithMany(i => i.Files)
                .HasForeignKey(a => a.DraftingInstructionID)
                .OnDelete(DeleteBehavior.Cascade);

            // ── One-to-many: DraftingInstruction → Draft
            modelBuilder.Entity<Draft>()
                .HasOne(d => d.DraftingInstruction)
                .WithMany(i => i.Drafts)
                .HasForeignKey(d => d.DraftingInstructionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DraftVersion>()
                .HasOne<DraftingInstruction>()
                .WithMany()
                .HasForeignKey(dv => dv.DraftingInstructionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InstructionLock>()
                .HasOne<DraftingInstruction>()
                .WithMany()
                .HasForeignKey(l => l.DraftingInstructionID)
                .OnDelete(DeleteBehavior.Cascade);

            // Apply explicit configurations for circulation entities and templates.
            modelBuilder.ApplyConfiguration(new CirculationLogConfiguration());
            modelBuilder.ApplyConfiguration(new CirculationResponseConfiguration());
            modelBuilder.ApplyConfiguration(new TemplateConfiguration());

            // Apply configuration for signatures
            modelBuilder.ApplyConfiguration(new SignatureConfiguration());

            // Comments configuration
            modelBuilder.ApplyConfiguration(new CommentConfiguration());

            // Additional model configuration can remain here.
        }
    }
}