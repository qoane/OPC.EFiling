using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Domain.Entities;
using OPC.EFiling.Infrastructure.Data.Configurations;

namespace OPC.EFiling.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Core domain sets
        public DbSet<Department> Departments { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }

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

        // Circulation
        public DbSet<CirculationLog> CirculationLogs { get; set; }
        public DbSet<CirculationResponse> CirculationResponses { get; set; }

        // Collaboration
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Signature> Signatures { get; set; }
        public DbSet<Template> Templates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // DraftingInstruction → Attachments
            modelBuilder.Entity<InstructionAttachment>()
                .HasOne(a => a.DraftingInstruction)
                .WithMany(i => i.Files)
                .HasForeignKey(a => a.DraftingInstructionID)
                .OnDelete(DeleteBehavior.Cascade);

            // DraftingInstruction → Drafts
            modelBuilder.Entity<Draft>()
                .HasOne(d => d.DraftingInstruction)
                .WithMany(i => i.Drafts)
                .HasForeignKey(d => d.DraftingInstructionID)
                .OnDelete(DeleteBehavior.Cascade);

            // DraftVersion → DraftingInstruction
            modelBuilder.Entity<DraftVersion>()
                .HasOne<DraftingInstruction>()
                .WithMany()
                .HasForeignKey(dv => dv.DraftingInstructionID)
                .OnDelete(DeleteBehavior.Cascade);

            // InstructionLock → DraftingInstruction
            modelBuilder.Entity<InstructionLock>()
                .HasOne<DraftingInstruction>()
                .WithMany()
                .HasForeignKey(l => l.DraftingInstructionID)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ CirculationLog → CirculationResponse (1:N)
            modelBuilder.Entity<CirculationResponse>()
                .HasOne(r => r.CirculationLog)
                .WithMany(l => l.Responses)
                .HasForeignKey(r => r.CirculationLogId)
                .OnDelete(DeleteBehavior.Cascade);

            // Signature → DraftingInstruction
            modelBuilder.Entity<Signature>()
                .HasOne<DraftingInstruction>()
                .WithMany()
                .HasForeignKey(s => s.DraftingInstructionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Apply configs
            modelBuilder.ApplyConfiguration(new CirculationLogConfiguration());
            modelBuilder.ApplyConfiguration(new CirculationResponseConfiguration());
            modelBuilder.ApplyConfiguration(new TemplateConfiguration());
            modelBuilder.ApplyConfiguration(new SignatureConfiguration());
            modelBuilder.ApplyConfiguration(new CommentConfiguration());
        }
    }
}
