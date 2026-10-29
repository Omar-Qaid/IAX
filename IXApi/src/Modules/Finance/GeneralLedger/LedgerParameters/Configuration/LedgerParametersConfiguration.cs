using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerParameters
{
    public class LedgerParametersConfiguration : IEntityTypeConfiguration<Entities.LedgerParameters>
    {
        public void Configure(EntityTypeBuilder<Entities.LedgerParameters> builder)
        {
            builder.ToTable("LedgerParameters");
            // One parameter record per company.
            builder.HasIndex(x => new { x.DataAreaId, x.Key_ }).IsUnique();
            builder.Property(x => x.MaxRoundingDifferenceMst).HasPrecision(28, 6);
            builder.Property(x => x.MaxRoundingDifferenceMstSecond).HasPrecision(28, 6);
        }
    }
}
