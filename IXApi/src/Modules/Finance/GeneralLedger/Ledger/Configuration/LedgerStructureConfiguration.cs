using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Ledger.Configuration
{
    public class LedgerStructureConfiguration : IEntityTypeConfiguration<LedgerStructure>
    {
        public void Configure(EntityTypeBuilder<LedgerStructure> builder)
        {
            builder.ToTable("LedgerStructure");
            // A structure is assigned to a ledger once, which is the standard table's own index.
            builder.HasIndex(x => new { x.Ledger, x.DimensionHierarchy }).IsUnique();
            builder.HasOne<Entities.Ledger>().WithMany()
                .HasForeignKey(x => x.Ledger).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<DimensionHierarchy>().WithMany()
                .HasForeignKey(x => x.DimensionHierarchy).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
