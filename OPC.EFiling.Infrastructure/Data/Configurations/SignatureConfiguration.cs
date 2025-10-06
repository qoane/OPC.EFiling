using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPC.EFiling.Domain.Entities;

namespace OPC.EFiling.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for the Signature entity. Defines table
    /// name and property mappings.
    /// </summary>
    public class SignatureConfiguration : IEntityTypeConfiguration<Signature>
    {
        public void Configure(EntityTypeBuilder<Signature> builder)
        {
            builder.ToTable("Signatures");
            builder.HasKey(s => s.SignatureId);
            builder.Property(s => s.SignerName).IsRequired().HasMaxLength(128);
            builder.Property(s => s.ImageData).IsRequired();
            builder.Property(s => s.SignedAt).IsRequired();
        }
    }
}