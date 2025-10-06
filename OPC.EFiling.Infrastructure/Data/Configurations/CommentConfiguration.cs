using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPC.EFiling.Domain.Entities;

namespace OPC.EFiling.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for Comment. Defines keys, indexes,
    /// relationships and table schema.
    /// </summary>
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");
            builder.HasKey(c => c.CommentId);
            builder.Property(c => c.AuthorName).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Text).IsRequired();
            builder.Property(c => c.Selection).HasMaxLength(1000);

            builder.Property(c => c.CreatedAt).IsRequired();

            builder.HasOne(c => c.ParentComment)
                   .WithMany(p => p.Replies)
                   .HasForeignKey(c => c.ParentCommentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => c.DraftingInstructionID);
        }
    }
}