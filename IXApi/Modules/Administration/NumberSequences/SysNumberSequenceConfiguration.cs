using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    public class SysNumberSequenceConfiguration : IEntityTypeConfiguration<SysNumberSequence>
    {
        public void Configure(EntityTypeBuilder<SysNumberSequence> builder)
        {
            builder.ToTable("SysNumberSequences");

            builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
            builder.Property(x => x.NameAR).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.EntityName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Prefix).HasMaxLength(20);
            builder.Property(x => x.Suffix).HasMaxLength(20);
            builder.Property(x => x.FormatPattern).HasMaxLength(100).IsRequired();
            builder.Property(x => x.TenantId).HasMaxLength(100);

            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => new { x.EntityName, x.TenantId }).IsUnique();
        }
    }
}
