using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPC.EFiling.Domain.Entities;

namespace OPC.EFiling.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for the CirculationResponse entity.
    /// Defines table name, keys, relationships and indexes.
    /// </summary>
    public class CirculationResponseConfiguration : IEntityTypeConfiguration<CirculationResponse>
    {
        public void Configure(EntityTypeBuilder<CirculationResponse> b)
        {
            b.ToTable("CirculationResponses");
            b.HasKey(x => x.CirculationResponseId);

            b.Property(x => x.ResponseText).HasMaxLength(2000);

            // Relationship: one circulation log has many responses
            b.HasOne(x => x.CirculationLog)
     .WithMany(c => c.Responses)
     .HasForeignKey(x => x.CirculationLogId)
     .OnDelete(DeleteBehavior.Cascade);


            // Relationship: optional link to the uploaded document returned by the ministry
            b.HasOne(x => x.Document)
                .WithMany()
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relationship: the OPC user who recorded the response
            b.HasOne(x => x.ReceivedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReceivedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            b.HasIndex(x => x.CirculationLogId);
            b.HasIndex(x => x.ReceivedAt);
        }
    }
}