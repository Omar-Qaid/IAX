using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class SalesQuotationLineConfiguration : IEntityTypeConfiguration<SalesQuotationLine>
    {
        public void Configure(EntityTypeBuilder<SalesQuotationLine> builder)
        {
            builder.ToTable("SalesQuotationLine");

        

            
        }
    }
}


