using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class CustPackingSlipJourConfiguration : IEntityTypeConfiguration<CustPackingSlipJour>
    {
        public void Configure(EntityTypeBuilder<CustPackingSlipJour> builder)
        {
            builder.HasIndex(x => x.PackingSlipId);
            builder.HasIndex(x => x.SalesId);
          
        }
    }
}

