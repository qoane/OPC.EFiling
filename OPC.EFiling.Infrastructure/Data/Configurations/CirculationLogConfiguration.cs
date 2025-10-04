using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPC.EFiling.Domain.Entities;

namespace OPC.EFiling.Infrastructure.Data.Configurations
{
    public class CirculationLogConfiguration : IEntityTypeConfiguration<CirculationLog>
    {
        public void Configure(EntityTypeBuilder<CirculationLog> b)
        {
            b.ToTable("CirculationLogs");
            b.HasKey(x => x.CirculationLogId);

            b.Property(x => x.SentToEmail).IsRequired().HasMaxLength(256);
            b.Property(x => x.CcEmail).HasMaxLength(256);
            b.Property(x => x.Subject).HasMaxLength(512);
            b.Property(x => x.VersionLabel).HasMaxLength(32);
            b.Property(x => x.Notes).HasMaxLength(2000);

            b.HasOne(x => x.Draft)
                .WithMany() // or .WithMany(d => d.CirculationLogs) if you add a collection on Draft
                .HasForeignKey(x => x.DraftId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.SentByUser)
                .WithMany()
                .HasForeignKey(x => x.SentByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            b.HasOne(x => x.Document)
                .WithMany()
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => x.DraftId);
            b.HasIndex(x => x.SentAt);
        }
    }
}
