using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    public class SysNumberSequenceConfiguration : IEntityTypeConfiguration<SysNumberSequence>
    {
        public void Configure(EntityTypeBuilder<SysNumberSequence> builder)
        {
            builder.ToTable("SysNumberSequences");

            builder.Property(x => x.NumberSequence).HasMaxLength(22).IsRequired();
            builder.Property(x => x.Txt).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Format).HasMaxLength(20).IsRequired();
            builder.Property(x => x.AnnotatedFormat).HasMaxLength(100).IsRequired();

            // Constraints
            builder.HasIndex(x => x.NumberSequence).IsUnique();
        }
    }
}
