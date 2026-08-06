using IAX.IXApi.Modules.ERP.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    public class SalesLineConfiguration : IEntityTypeConfiguration<SalesLine>
    {
        public void Configure(EntityTypeBuilder<SalesLine> builder)
        {
            builder.ToTable("SalesLine");


        }
    }
}
