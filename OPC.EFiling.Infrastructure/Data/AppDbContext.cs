using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Domain.Entities;

namespace OPC.EFiling.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Department> Departments { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }

        // Existing set
        public DbSet<DraftingInstruction> DraftingInstructions { get; set; }

        // Add these two new DbSet lines:
        public DbSet<InstructionAttachment> InstructionAttachments { get; set; }
        public DbSet<Draft> Drafts { get; set; }

        public DbSet<Document> Documents { get; set; }
        public DbSet<UploadedFile> Files { get; set; }
        public DbSet<ApprovalLog> ApprovalLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<InstructionLock> InstructionLocks { get; set; }
        public DbSet<DraftVersion> DraftVersions { get; set; }

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

            // If you have any existing relationships to configure (e.g. for Documents or UploadedFile),
            // you can keep them here as well.
        }

    }
}