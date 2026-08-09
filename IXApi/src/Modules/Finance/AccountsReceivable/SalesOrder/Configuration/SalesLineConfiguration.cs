using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class SalesLineConfiguration : IEntityTypeConfiguration<SalesLine>
    {
        public void Configure(EntityTypeBuilder<SalesLine> builder)
        {
            builder.ToTable("SalesLine");


        }
    }
}

