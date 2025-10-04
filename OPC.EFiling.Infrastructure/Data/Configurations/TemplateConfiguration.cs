using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPC.EFiling.Domain.Entities;

namespace OPC.EFiling.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for the <see cref="Template"/> entity. Defines table name, keys and
    /// column lengths. Templates are stored in the "Templates" table.
    /// </summary>
    public class TemplateConfiguration : IEntityTypeConfiguration<Template>
    {
        public void Configure(EntityTypeBuilder<Template> builder)
        {
            builder.ToTable("Templates");

            builder.HasKey(t => t.TemplateId);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Description)
                .HasMaxLength(1000);

            builder.Property(t => t.FilePath)
                .IsRequired();

            builder.Property(t => t.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}